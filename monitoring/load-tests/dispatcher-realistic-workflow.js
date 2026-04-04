import http from 'k6/http';
import { check, sleep } from 'k6';
import { Counter, Rate } from 'k6/metrics';

const BASE_URL = __ENV.BASE_URL || 'http://host.docker.internal:8080';
const CUSTOMER_COUNT = Number(__ENV.CUSTOMER_COUNT || 5);
const FLIGHT_COUNT = Number(__ENV.FLIGHT_COUNT || 4);
const SEATS_PER_FLIGHT = Number(__ENV.SEATS_PER_FLIGHT || 3000);
const HOT_FLIGHT_SEATS = Number(__ENV.HOT_FLIGHT_SEATS || 120);

const businessConflicts = new Counter('business_conflicts');
const successfulReservations = new Counter('successful_reservations');
const successfulCheckouts = new Counter('successful_checkouts');
const systemFailureRate = new Rate('system_failure_rate');

function jsonHeaders(token) {
  return {
    headers: {
      Authorization: `Bearer ${token}`,
      'Content-Type': 'application/json'
    }
  };
}

function authOnly(token) {
  return {
    headers: {
      Authorization: `Bearer ${token}`
    }
  };
}

function randomSeat() {
  const row = Math.floor(Math.random() * SEATS_PER_FLIGHT) + 1;
  const letters = ['A', 'B', 'C', 'D', 'E', 'F'];
  return `${row}${letters[Math.floor(Math.random() * letters.length)]}`;
}

function hotspotSeat() {
  const row = Math.floor(Math.random() * Math.ceil(HOT_FLIGHT_SEATS / 6)) + 1;
  const letters = ['A', 'B', 'C', 'D', 'E', 'F'];
  return `${row}${letters[Math.floor(Math.random() * letters.length)]}`;
}

function buildCustomerEmail(index) {
  return `loadtest_${Date.now()}_${index}_${Math.floor(Math.random() * 100000)}@system.local`;
}

function randomItem(items) {
  return items[Math.floor(Math.random() * items.length)];
}

export const options = {
  setupTimeout: '180s',
  thresholds: {
    http_req_failed: ['rate<0.30']
  }
};

export function setup() {
  const adminLogin = http.post(
    `${BASE_URL}/api/auth/login`,
    JSON.stringify({
      email: 'admin@system.local',
      password: 'Admin123!'
    }),
    { headers: { 'Content-Type': 'application/json' } }
  );

  check(adminLogin, {
    'admin login ok': (res) => res.status === 200,
    'admin login token': (res) => !!res.json('token')
  });

  const adminToken = adminLogin.json('token');
  const adminHeaders = jsonHeaders(adminToken);
  const customers = [];
  const flights = [];

  for (let index = 0; index < CUSTOMER_COUNT; index += 1) {
    const customerEmail = buildCustomerEmail(index);
    const customerPassword = 'Customer123!';

    const register = http.post(
      `${BASE_URL}/api/auth/register`,
      JSON.stringify({
        email: customerEmail,
        password: customerPassword,
        role: 'Customer'
      }),
      { headers: { 'Content-Type': 'application/json' } }
    );

    check(register, {
      [`customer ${index} register accepted`]: (res) => res.status === 200 || res.status === 400
    });

    const customerLogin = http.post(
      `${BASE_URL}/api/auth/login`,
      JSON.stringify({
        email: customerEmail,
        password: customerPassword
      }),
      { headers: { 'Content-Type': 'application/json' } }
    );

    check(customerLogin, {
      [`customer ${index} login ok`]: (res) => res.status === 200,
      [`customer ${index} login token`]: (res) => !!res.json('token')
    });

    customers.push({
      email: customerEmail,
      token: customerLogin.json('token')
    });
  }

  for (let index = 0; index < FLIGHT_COUNT; index += 1) {
    const createFlight = http.post(
      `${BASE_URL}/api/flights`,
      JSON.stringify({
        from: ['IST', 'ADB', 'AYT'][index % 3],
        to: ['ESB', 'SAW', 'TZX'][index % 3],
        departureTime: `2026-05-0${index + 1}T08:30:00Z`,
        arrivalTime: `2026-05-0${index + 1}T09:45:00Z`,
        price: 1500 + (index * 250),
        availableSeatCount: index === 0 ? HOT_FLIGHT_SEATS : SEATS_PER_FLIGHT
      }),
      adminHeaders
    );

    check(createFlight, {
      [`flight ${index} created`]: (res) => res.status === 201,
      [`flight ${index} id exists`]: (res) => !!res.json('id')
    });

    flights.push({
      id: createFlight.json('id')
    });
  }

  return {
    adminToken,
    customers,
    flights,
    hotspotFlightId: flights[0].id
  };
}

export default function (data) {
  const adminHeaders = jsonHeaders(data.adminToken);
  const customer = randomItem(data.customers);
  const customerHeaders = jsonHeaders(customer.token);
  const customerAuth = authOnly(customer.token);
  const flight = randomItem(data.flights);
  const roll = Math.random();

  if (roll < 0.50) {
    const response = http.get(`${BASE_URL}/api/flights`, customerAuth);
    systemFailureRate.add(response.status >= 500);
    check(response, {
      'get flights ok': (res) => res.status === 200
    });
  } else if (roll < 0.70) {
    const response = http.get(`${BASE_URL}/api/flights/${flight.id}`, customerAuth);
    systemFailureRate.add(response.status >= 500);
    check(response, {
      'get flight detail ok': (res) => res.status === 200
    });
  } else if (roll < 0.85) {
    const response = http.get(`${BASE_URL}/api/availability/${flight.id}`, customerAuth);
    systemFailureRate.add(response.status >= 500);
    check(response, {
      'get availability ok': (res) => res.status === 200
    });
  } else if (roll < 0.95) {
    const selectedFlightId = Math.random() < 0.70 ? data.hotspotFlightId : flight.id;
    const seatNumber = selectedFlightId === data.hotspotFlightId ? hotspotSeat() : randomSeat();
    const response = http.post(
      `${BASE_URL}/api/reservations`,
      JSON.stringify({
        flightId: selectedFlightId,
        passengerName: `Load Tester ${__VU}-${__ITER}`,
        seatNumber
      }),
      customerHeaders
    );

    if (response.status === 409) {
      businessConflicts.add(1);
    }

    systemFailureRate.add(response.status >= 500);

    check(response, {
      'reservation call acceptable': (res) => [201, 409, 503].includes(res.status)
    });

    if (response.status === 201) {
      successfulReservations.add(1);
    }
  } else {
    const selectedFlightId = Math.random() < 0.70 ? data.hotspotFlightId : flight.id;
    const seatNumber = selectedFlightId === data.hotspotFlightId ? hotspotSeat() : randomSeat();
    const reservation = http.post(
      `${BASE_URL}/api/reservations`,
      JSON.stringify({
        flightId: selectedFlightId,
        passengerName: `Checkout Tester ${__VU}-${__ITER}`,
        seatNumber
      }),
      customerHeaders
    );

    if (reservation.status === 409) {
      businessConflicts.add(1);
    }

    systemFailureRate.add(reservation.status >= 500);

    check(reservation, {
      'checkout reservation acceptable': (res) => [201, 409, 503].includes(res.status)
    });

    if (reservation.status === 201) {
      successfulReservations.add(1);
      const paymentId = reservation.json('paymentId');

      if (paymentId) {
        const paymentGet = http.get(`${BASE_URL}/api/payments/${paymentId}`, customerAuth);
        systemFailureRate.add(paymentGet.status >= 500);
        check(paymentGet, {
          'payment fetch ok': (res) => res.status === 200
        });

        const paymentPatch = http.patch(
          `${BASE_URL}/api/payments/${paymentId}`,
          JSON.stringify({ status: 'Completed' }),
          customerHeaders
        );

        systemFailureRate.add(paymentPatch.status >= 500);
        check(paymentPatch, {
          'payment complete acceptable': (res) => res.status === 204 || res.status === 400
        });

        if (paymentPatch.status === 204) {
          successfulCheckouts.add(1);
        }
      }
    }
  }

  if (roll > 0.97) {
    const response = http.get(`${BASE_URL}/api/auth/me`, adminHeaders);
    systemFailureRate.add(response.status >= 500);
    check(response, {
      'admin me ok': (res) => res.status === 200
    });
  }

  sleep(0.2);
}

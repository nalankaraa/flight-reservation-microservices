import http from 'k6/http';
import { check, sleep } from 'k6';

const BASE_URL = __ENV.BASE_URL || 'http://host.docker.internal:8080';
const SEEDED_TOKEN = __ENV.TOKEN;
const SEEDED_FLIGHT_ID = __ENV.FLIGHT_ID;

function authHeaders(token) {
  return {
    headers: {
      Authorization: `Bearer ${token}`,
      'Content-Type': 'application/json'
    }
  };
}

export function setup() {
  if (SEEDED_TOKEN && SEEDED_FLIGHT_ID) {
    return { token: SEEDED_TOKEN, flightId: SEEDED_FLIGHT_ID };
  }

  const loginResponse = http.post(
    `${BASE_URL}/api/auth/login`,
    JSON.stringify({
      email: 'admin@system.local',
      password: 'Admin123!'
    }),
    {
      headers: { 'Content-Type': 'application/json' }
    }
  );

  check(loginResponse, {
    'setup login succeeded': (response) => response.status === 200,
    'setup login returned token': (response) => !!response.json('token')
  });

  const token = loginResponse.json('token');
  const headers = authHeaders(token);

  let flightsResponse = http.get(`${BASE_URL}/api/flights`, headers);
  check(flightsResponse, {
    'initial flights lookup succeeded': (response) => response.status === 200
  });

  let flights = flightsResponse.json();
  let flightId = Array.isArray(flights) && flights.length > 0 ? flights[0].id : null;

  if (!flightId) {
    const uniqueSuffix = `${Date.now()}`;
    const createFlightResponse = http.post(
      `${BASE_URL}/api/flights`,
      JSON.stringify({
        from: `IST-${uniqueSuffix}`,
        to: `ESB-${uniqueSuffix}`,
        departureTime: '2026-05-01T08:30:00Z',
        arrivalTime: '2026-05-01T09:45:00Z',
        price: 1499,
        availableSeatCount: 120
      }),
      headers
    );

    check(createFlightResponse, {
      'flight creation succeeded': (response) => response.status === 201
    });

    flightId = createFlightResponse.json('id');
  }

  return { token, flightId };
}

export const options = {
  setupTimeout: '120s',
  thresholds: {
    http_req_failed: ['rate<0.20']
  }
};

export default function (data) {
  const headers = authHeaders(data.token);
  const choice = Math.random();

  let response;

  if (choice < 0.55) {
    response = http.get(`${BASE_URL}/api/flights`, headers);
  } else if (choice < 0.8) {
    response = http.get(`${BASE_URL}/api/auth/me`, headers);
  } else {
    response = http.get(`${BASE_URL}/api/availability/${data.flightId}/seats`, headers);
  }

  check(response, {
    'dispatcher request succeeded': (currentResponse) =>
      currentResponse.status >= 200 && currentResponse.status < 400
  });

  sleep(0.2);
}

# Yazılım Geliştirme Laboratuvarı-II · Proje 1


| Alan | Bilgi |
|---|---|
| Proje Adı | Flight Reservation Microservices |
| Geliştirici Ekip | Esma Nur Mantı - Nalan Kara |
| Tarih | 05.04.2026 |

---

## 1. Giriş ve Amaç

### Problemin Tanımı

Dağıtık yapılarda her servisin ayrı ayrı kimlik doğrulama, loglama, yönlendirme ve hata yönetimi üstlenmesi; bakım maliyetini artırır, güvenlik kurallarını parçalı hale getirir ve yük altında sistem davranışının izlenmesini zorlaştırır. Birden fazla servisin dış dünyaya açık olması da hem operasyonel karmaşıklık hem de saldırı yüzeyi oluşturur.

### Proje Amacı

Bu proje; tüm istemci trafiğini merkezi bir `Dispatcher` API Gateway üzerinden alan, mikroservisleri tek bir giriş noktasının arkasında toplayan, JWT tabanlı güvenliği merkezileştiren, servis bazlı MongoDB ayrımını koruyan ve web tabanlı dashboard ile gözlemlenebilirlik sağlayan bir uçuş rezervasyon sistemi geliştirmeyi amaçlar.

### Temel Hedefler

- Dispatcher üzerinden merkezi yönlendirme ve güvenlik
- En az 4 mikroservis içeren ölçeklenebilir bir mimari
- Her servis için ayrı NoSQL veritabanı yaklaşımı
- `docker compose up --build` ile tek komutta ayağa kalkan sistem
- TDD odaklı test altyapısı
- k6 ile profesyonel yük testi
- Grafana ve özel dashboard ile canlı trafik görselleştirmesi

### Teknoloji Yığını

| Bileşen | Teknoloji | Sürüm |
|---|---|---|
| Dispatcher | ASP.NET Core Web API | .NET 9 |
| Mikroservisler | ASP.NET Core Web API | .NET 9 |
| Ortak güvenlik | JWT Bearer Authentication | .NET ekosistemi |
| Veritabanı | MongoDB | 7.0 |
| Görselleştirme | Grafana | 11.2.0 |
| Yük Testi | k6 | script tabanlı |
| API Dokümantasyonu | Swagger / OpenAPI | Swashbuckle |
| Test | xUnit | .NET 9 uyumlu |

---

## 2. Sistem Tasarımı ve Mimari

### Genel Mimari

```mermaid
flowchart LR
    Client["İstemci / Tarayıcı"]

    subgraph Public["Dışa Açık Katman"]
        Dispatcher["Dispatcher\nAPI Gateway\n:8080"]
        Grafana["Grafana\n:3000"]
    end

    subgraph Services["Mikroservis Katmanı"]
        Auth["AuthService"]
        Flight["FlightService"]
        Availability["AvailabilityService"]
        Reservation["ReservationService"]
        Payment["PaymentService"]
        Notification["NotificationService"]
    end

    subgraph Data["Veri Katmanı"]
        DDB[("dispatcher-db")]
        ADB[("auth-db")]
        FDB[("flight-db")]
        AVDB[("availability-db")]
        RDB[("reservation-db")]
        PDB[("payment-db")]
        NDB[("notification-db")]
    end

    Client --> Dispatcher
    Client -. gözlem .-> Grafana

    Dispatcher --> Auth
    Dispatcher --> Flight
    Dispatcher --> Availability
    Dispatcher --> Reservation
    Dispatcher --> Payment
    Dispatcher --> Notification

    Dispatcher --> DDB
    Auth --> ADB
    Flight --> FDB
    Availability --> AVDB
    Reservation --> RDB
    Payment --> PDB
    Notification --> NDB
```

Bu yapı, sistemi üç net katmanda düşünmeyi kolaylaştırır:

- dış dünyaya açık giriş noktaları
- iş kurallarını barındıran mikroservisler
- servislerden izole veri katmanı

### Dispatcher Akışı

```mermaid
flowchart TD
    A["İstek geldi"] --> B{"Yol auth mu?"}
    B -- Evet --> C["Auth rotasına yönlendir"]
    B -- Hayır --> D["JWT doğrula"]
    D --> E{"Token geçerli mi?"}
    E -- Hayır --> F["401 Unauthorized"]
    E -- Evet --> G["Rol ve rota kuralını çöz"]
    G --> H["İsteği logla"]
    H --> I{"Hedef servis bulundu mu?"}
    I -- Hayır --> J["404 Not Found"]
    I -- Evet --> K["İsteği hedef servise ilet"]
    K --> L{"Servis erişilebilir mi?"}
    L -- Hayır --> M["502 / 503"]
    L -- Evet --> N["Yanıtı döndür"]
    N --> O["Request log ve duration kaydı"]
    O --> P["İstemciye yanıt"]
```

### Mimari Özeti

- `Dispatcher`, sistemin tek public giriş noktası olarak çalışır.
- Mikroservisler host makineye `ports` ile açılmaz; compose içinde `expose` kullanılır.
- Her servis kendi MongoDB instance'ına sahiptir.
- Reservation, Availability, Payment ve Notification servisleri arasında iş akışı vardır.
- Dispatcher hem request log tutar hem de dashboard üzerinden canlı trafik özeti sunar.

---

## 3. Richardson Olgunluk Modeli

Richardson Olgunluk Modeli, REST servislerinin URI tasarımı, HTTP metotları ve hypermedia kullanımı açısından olgunluk seviyesini tanımlar.

```mermaid
graph LR
    L0["Seviye 0<br/>tek endpoint"]
    L1["Seviye 1<br/>kaynak bazlı URI"]
    L2["Seviye 2<br/>HTTP metotları + status code"]
    L3["Seviye 3<br/>HATEOAS"]

    L0 --> L1 --> L2 --> L3
    style L2 fill:#8fd19e,color:#000
```

### Projede Uygulanan Seviye

Bu proje temelde **RMM Seviye 2** hedefini karşılar:

- Kaynak odaklı URI tasarımı vardır.
- `GET`, `POST`, `PUT`, `PATCH`, `DELETE` metotları anlamsal olarak kullanılır.
- Uygun `2xx`, `4xx` ve `5xx` durum kodları döndürülür.

### Örnek Rota ve Durum Kodları

| Kaynak | URI | HTTP Metodu | Başarı | Hata |
|---|---|---|---|---|
| Kayıt | `/api/auth/register` | POST | 200 | 400 |
| Giriş | `/api/auth/login` | POST | 200 | 401 |
| Profil | `/api/auth/me` | GET | 200 | 401, 404 |
| Uçuş listeleme | `/api/flights` | GET | 200 | 401, 403 |
| Uçuş oluşturma | `/api/flights` | POST | 201 | 400, 401, 403 |
| Uçuş güncelleme | `/api/flights/{id}` | PUT | 204 | 401, 403, 404 |
| Uçuş silme | `/api/flights/{id}` | DELETE | 204 | 401, 403, 404 |
| Koltuk durumu | `/api/availability/{flightId}` | GET | 200 | 401, 403 |
| Hold koyma | `/api/availability/{flightId}/seats/{seat}/hold` | PUT | 200 | 401, 403, 409 |
| Rezervasyon oluşturma | `/api/reservations` | POST | 201 | 400, 401, 404, 409, 503 |
| Ödeme güncelleme | `/api/payments/{id}` | PATCH | 204 | 400, 401, 403, 404 |

---

## 4. Servis Yapıları ve Sorumlulukları

### Katmanlı Organizasyon

Her servis benzer bir katmanlı yapı ile düzenlenmiştir:

- `*.Api`
- `*.Application`
- `*.Domain`
- `*.Infrastructure`
- `*.Tests`

Bu yapı:

- sorumluluk ayrımı,
- test edilebilirlik,
- değişikliklerin izole edilmesi,
- altyapı kodunun iş kurallarından ayrılması

için tercih edilmiştir.

### Dispatcher

Dispatcher katmanı şu teknik görevleri üstlenir:

- route çözme ve proxy işlemi
- JWT doğrulama
- rol tabanlı güvenlik
- request log kaydı
- dashboard ve log API sunumu
- canlı trafik için grafikler

### Mikroservislerin Ana Sorumlulukları

| Servis | Ana Sorumluluk |
|---|---|
| `AuthService` | kullanıcı kaydı, login, token ile kullanıcı bilgisi döndürme |
| `FlightService` | uçuş CRUD işlemleri |
| `AvailabilityService` | koltuk uygunluğu, hold ve rezervasyon durumu |
| `ReservationService` | rezervasyon oluşturma ve kullanıcı rezervasyonlarını listeleme |
| `PaymentService` | ödeme oluşturma ve durum güncelleme |
| `NotificationService` | bildirim oluşturma, gönderme ve okundu işaretleme |

---

## 5. Sequence Diyagramları

### 5.1 Kullanıcı Girişi

```mermaid
sequenceDiagram
    actor Client
    participant D as Dispatcher
    participant A as AuthService
    participant DB as auth-db

    Client->>D: POST /api/auth/login
    D->>A: forward login request
    A->>DB: kullanıcı sorgusu
    DB-->>A: kullanıcı kaydı
    A-->>D: token response
    D-->>Client: 200 OK + JWT
```

### 5.2 Rezervasyon Oluşturma

```mermaid
sequenceDiagram
    actor Client
    participant D as Dispatcher
    participant R as ReservationService
    participant AV as AvailabilityService
    participant P as PaymentService

    Client->>D: POST /api/reservations
    D->>D: JWT doğrula
    D->>R: rezervasyon isteğini ilet
    R->>AV: koltuk uygunluğunu doğrula / hold işlemi
    AV-->>R: uygun / değil
    alt uygun
        R->>P: ödeme kaydı oluştur
        P-->>R: payment id
        R-->>D: 201 Created
        D-->>Client: rezervasyon bilgisi
    else çakışma
        R-->>D: 409 Conflict
        D-->>Client: conflict response
    end
```

### Seat Conflict Akışı

Seat conflict akışı, sistemin en kritik yarış durumu senaryosunu temsil eder. Aynı koltuğa birden fazla isteğin eşzamanlı gelmesi halinde veri tutarlılığının korunması için hem `AvailabilityService` hem de `ReservationService` tarafında ek kontroller uygulanır.

```mermaid
flowchart TD
    A["Reservation isteği gelir"] --> B["Seat number normalize edilir"]
    B --> C{"Aynı koltuk daha önce alınmış mı?"}
    C -- Evet --> D["409 Conflict"]
    C -- Hayır --> E["Availability hold işlemi başlatılır"]
    E --> F{"Hold başarılı mı?"}
    F -- Hayır --> G["AvailabilityUnavailable / SeatConflict"]
    F -- Evet --> H["Reservation repository'ye yazılır"]
    H --> I{"Duplicate insert oluştu mu?"}
    I -- Evet --> J["Hold bırakılır + 409 Conflict"]
    I -- Hayır --> K["Seat confirm işlemi yapılır"]
    K --> L["Flight fiyatı alınır"]
    L --> M["Payment oluşturulur"]
    M --> N["201 Created"]
```

Bu akışın temel amacı:

- aynı koltuğun iki farklı kullanıcıya satılmasını engellemek,
- hold başarısız olduğunda erken reddetmek,
- duplicate insert durumunda geri alma işlemi yaparak veri tutarlılığını korumaktır.

### 5.3 Ödeme Tamamlama ve Bildirim

```mermaid
sequenceDiagram
    actor Client
    participant D as Dispatcher
    participant P as PaymentService
    participant N as NotificationService

    Client->>D: PATCH /api/payments/{id}
    D->>P: ödeme durumunu güncelle
    P->>N: bildirim oluştur / gönder
    N-->>P: notification saved
    P-->>D: 204 No Content
    D-->>Client: 204 No Content
```

---

## 6. Veri Katmanı Tasarımı

### Veritabanı Yaklaşımı

Her servis kendi verisini kendi MongoDB örneğinde tutar. Böylece servisler birbirlerinin veritabanına doğrudan bağımlı olmaz.

| Servis | Veritabanı |
|---|---|
| Dispatcher | `dispatcher-db` |
| AuthService | `auth-db` |
| FlightService | `flight-db` |
| AvailabilityService | `availability-db` |
| ReservationService | `reservation-db` |
| PaymentService | `payment-db` |
| NotificationService | `notification-db` |

### Mantıksal Veri İlişkileri

```mermaid
erDiagram
    USER {
        string id
        string email
        string passwordHash
        string role
    }

    FLIGHT {
        string id
        string from
        string to
        datetime departureTime
        datetime arrivalTime
        decimal price
        int availableSeatCount
    }

    SEAT_HOLD {
        string id
        string flightId
        string seatNumber
        string userId
        string status
    }

    RESERVATION {
        string id
        string userId
        string flightId
        string seatNumber
        string paymentId
        string passengerName
    }

    PAYMENT {
        string id
        string userId
        string reservationId
        string status
    }

    NOTIFICATION {
        string id
        string userId
        string message
        string status
    }

    USER ||--o{ RESERVATION : creates
    FLIGHT ||--o{ RESERVATION : contains
    FLIGHT ||--o{ SEAT_HOLD : has
    RESERVATION ||--|| PAYMENT : owns
    USER ||--o{ NOTIFICATION : receives
```

### Dispatcher Veri Kullanımı

Dispatcher servisinin MongoDB kullandığı iki ana alan vardır:

- route tanımlarının saklanması
- request log verilerinin tutulması

---

## 7. TDD ve Test Yaklaşımı

Proje boyunca TDD mantığına uygun olacak şekilde testlenebilir sınıf tasarımına öncelik verilmiştir. Dispatcher özelinde test önce, sonra implementasyon yaklaşımına dair commit kanıtları mevcuttur.

### Test Projeleri

| Proje | Test Dizini |
|---|---|
| Dispatcher | `gateway/Dispatcher/Dispatcher.Tests` |
| AuthService | `services/AuthService/AuthService.Tests` |
| FlightService | `services/FlightService/FlightService.Tests` |
| AvailabilityService | `services/AvailabilityService/AvailabilityService.Tests` |
| ReservationService | `services/ReservationService/ReservationService.Tests` |
| PaymentService | `services/PaymentService/PaymentService.Tests` |
| NotificationService | `services/NotificationService/NotificationService.Tests` |

### TDD Kanıtları


<img width="1553" height="550" alt="red green refactor" src="https://github.com/user-attachments/assets/35890e35-44cd-4796-99d1-f9df2b73e3f0" />

---

## 8. Test Senaryoları ve Kapsamı

### Dispatcher Testleri

Öne çıkan test başlıkları:

- `AuthorizationTests`
- `DatabaseRouteResolverTests`
- `DispatcherMetricsStoreTests`
- `ErrorHandlingTests`
- `ForwardingTests`
- `LoggingTests`
- `RequestLogsControllerTests`
- `EndToEndWorkflowTests`

### Mikroservis Testleri

Projede şu senaryolar test edilmiştir:

- başarılı login ve token üretimi
- yetkisiz erişimin reddedilmesi
- süresi dolmuş token'in reddedilmesi
- route çözümleme
- seat conflict
- reservation + payment + notification zinciri
- payment ownership kontrolü
- notification ownership kontrolü
- Mongo repository davranışları

### Sonuç Değerlendirmesi

Test mimarisi, projenin sadece controller düzeyinde değil; repository, application service ve entegrasyon davranışı seviyesinde de güvenceye alındığını gösterir.

---

## 9. Yük Testi Sonuçları

Yük testleri `monitoring/load-tests` altındaki k6 senaryoları ile gerçekleştirilmiştir. Sistemde ayrıca gerçekçi kullanım ağırlıkları içeren `dispatcher-realistic-workflow.js` senaryosu yer alır.

### Ölçülen Sonuçlar

`monitoring/load-tests/results-realistic.json` dosyasındaki güncel değerler:

<img width="1201" height="389" alt="image" src="https://github.com/user-attachments/assets/43344282-a701-4266-876d-b1aade91943c" />


### Sonuçların Yorumu

- Trafik arttıkça latency yükselmektedir; bu beklenen davranıştır.
- `results-realistic.json` notlarına göre sistem kaynaklı `5xx` oranı `%0` seviyesindedir.
- Görülen hata oranı büyük ölçüde iş kuralı çatışmaları ve koltuk rekabetinden kaynaklanan `409` benzeri durumları temsil eder.
- Sistem, 500 eşzamanlı kullanıcı altında dahi ana iş akışlarını sürdürebilmektedir.

---

## 10. Monitoring ve Gözlemlenebilirlik

Projenin güçlü yönlerinden biri Dispatcher üzerinde toplanan gözlemlenebilirlik katmanıdır.

### Sağlanan Bileşenler

- Grafana dashboard  
  <img width="1604" height="816" alt="Grafana dashboard" src="https://github.com/user-attachments/assets/ccb51fca-49c2-4a79-a146-1bd879063705" />

- Web tabanlı monitoring ekranı ve servis bazlı trafik dağılımı
  <img width="1243" height="872" alt="Monitoring dashboard" src="https://github.com/user-attachments/assets/c3650b72-b4db-44c2-a3ba-3e2069e8b917" />

- Detaylı log tablosu  
  <img width="1164" height="518" alt="Detaylı log tablosu" src="https://github.com/user-attachments/assets/4bdd24fa-2a7a-4cd2-aa91-75c38410e909" />

- Load test sonuçları API: 
  <img width="1230" height="805" alt="Yük testi sonuçları" src="https://github.com/user-attachments/assets/30ceb15f-73fa-440b-bebe-51f8a6e21d0b" />

### Dashboard Özellikleri

Dispatcher içindeki HTML tabanlı dashboard şu verileri gösterebilir:

- canlı trafik özet kartları
- toplam istek
- başarılı / başarısız istek sayısı
- ortalama duration
- hata oranı
- servis bazlı trafik dağılımı
- son request logları
- Grafana panel embed alanları
- yük testi özet kartları
- yük testi sonuç tablosu

---

## 11. Docker ve Sistem Orkestrasyonu

Tüm sistem tek komutla ayağa kalkar:

```powershell
docker-compose up --build
```

Compose dosyası şu bileşenleri orkestre eder:

- Dispatcher
- AuthService
- FlightService
- AvailabilityService
- ReservationService
- PaymentService
- NotificationService
- 7 ayrı MongoDB instance'ı
- Grafana

---

## 12. Ağ Yapısı ve İzolasyon

Sistem `backend` isimli Docker ağı üzerinde çalışır. Compose tanımına göre:

- Dispatcher dış dünyaya `8080` portu ile açılır.
- Grafana izleme amacıyla dışa açıktır.
- Mikroservisler `ports` ile publish edilmez; sadece `expose` edilir.
- Böylece servisler compose içi ağda görünür, host'tan doğrudan çağrılmaz.

### Değerlendirme

Projede tek backend network yaklaşımı kullanılmıştır. Ancak dış erişim kontrolü yine korunmuştur; çünkü uygulama servisleri host portu yayınlamaz.

---

## 13. Teknik Değerlendirme

### Güçlü Yönler

- 6 mikroservis + 1 merkezi gateway ile ders isterini fazlasıyla karşılar.
- Her servis için ayrı MongoDB kullanımı vardır.
- Güvenlik ve route yönetimi Dispatcher üzerinde merkezileştirilmiştir.
- Gözlemlenebilirlik yalnızca Grafana ile sınırlı değildir; özel dashboard da vardır.
- Reservation, Availability, Payment ve Notification arasında gerçek iş akışı kurulmuştur.
- Test altyapısı proje geneline yayılmıştır.

### Sınırlılıklar

- Tüm sistem tek compose dosyası içinde çalıştığı için ileri ölçekleme senaryoları ayrıca ele alınmamıştır.
- Distributed tracing yoktur.
- Rate limiting ve circuit breaker gibi ileri gateway özellikleri henüz eklenmemiştir.
- Log saklama stratejisi zamanla daha da zenginleştirilebilir.

### Geliştirme Fikirleri

- distributed tracing eklemek
- rate limiting eklemek
- circuit breaker ve retry politikaları eklemek
- event-driven mimari ile servisler arası coupling'i azaltmak
- yönetici / müşteri arayüzünü görsel olarak genişletmek

---

## 15. Kurulum ve Çalıştırma

### Gereksinimler

| Araç | Sürüm |
|---|---|
| Docker Desktop | güncel |
| Docker Compose | v2+ |
| .NET SDK | 9.0 |

### 1. Repoyu klonla

```bash
git clone https://github.com/nalankaraa/flight-reservation-microservices.git
cd flight-reservation-microservices
```

### 2. Sistemi başlat

```bash
docker compose up --build
```

Arka planda çalıştırmak için:

```bash
docker compose up --build -d
```

### 3. Swagger portlarını da açmak istersen

```bash
docker compose -f docker-compose.yml -f docker-compose.swagger.yml up --build
```

### 4. Servisleri doğrula

```bash
docker compose ps
```

### 5. Varsayılan yönetici hesabı

```text
E-posta: admin@system.local
Şifre:   Admin123!
```

### 6. Sistemi durdur

```bash
docker compose down
```

Volume'ları da silmek istersen:

```bash
docker compose down -v
```

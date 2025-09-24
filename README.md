## Direcional Imobiliária - API (.NET 9) com DDD, CQRS, MediatR e RabbitMQ

### Visão Geral
API de exemplo para o desafio técnico da Direcional Engenharia, construída com boas práticas: DDD/EDD, CQRS com MediatR, EF Core, JWT, Serilog, Swagger e RabbitMQ (MassTransit) para eventos de domínio.

Serviços via Docker Compose:
- SQL Server Express (banco DirecionalDb)
- RabbitMQ (management em `http://localhost:15672`)
- API em `http://localhost:8080`

### Requisitos
- Docker e Docker Compose instalados

### Subindo o projeto
```bash
docker compose up -d --build
```

- Acesse Swagger: `http://localhost:8080/swagger`
- RabbitMQ UI: `http://localhost:15672` (user: guest / pass: guest)

### Autenticação (JWT)
Endpoint de login:
```
POST /auth/login
{
  "username": "admin",
  "password": "admin123"
}
```
Resposta:
```
{
  "token": "<JWT>",
  "expiresAtUtc": "..."
}
```
Use o token em Authorization: `Bearer <JWT>` no Swagger (Authorize) e em todas as requisições protegidas.

### Dados iniciais (seed)
- Usuário admin (admin/admin123)
- Apartamentos: A-101, A-102, B-201
- Cliente exemplo (para testes) 

Caso precise resemear em dev, há um endpoint protegido:
```
POST /dev/seed
```

### Endpoints principais
- Auth
  - POST `/auth/login`
- Clients (protegido)
  - GET `/clients?page=1&pageSize=20&search=`
  - POST `/clients` { name, email, document, phone }
  - PUT `/clients/{id}` { name, email, document, phone }
  - DELETE `/clients/{id}`
- Apartments (protegido)
  - GET `/apartments?page=1&pageSize=20&search=`
  - POST `/apartments` { code, block, floor, number, price }
  - PUT `/apartments/{id}` { code, block, floor, number, price }
  - DELETE `/apartments/{id}`
- Reservations (protegido)
  - POST `/reservations` { clientId, apartmentId, expiresHours }
  - DELETE `/reservations/{id}`
- Sales (protegido)
  - POST `/sales` { clientId, apartmentId, reservationId?, downPayment, totalPrice }

### Migrações (EF Core)
- Em Docker, as migrações são aplicadas automaticamente no startup (Database.Migrate()).
- Para criar/aplicar migrações em desenvolvimento local:
```bash
# Instale a CLI (se necessário)
dotnet tool install --global dotnet-ef

# Criar migração (ajuste o nome)
dotnet ef migrations add AddNovaFuncionalidade \
  -p Direcional.Infrastructure \
  -s Direcional.Api

# Aplicar no banco local
dotnet ef database update \
  -p Direcional.Infrastructure \
  -s Direcional.Api
```
Observações:
- O projeto de Startup (-s) aponta para `Direcional.Api` (onde está a configuração e DI).
- O projeto de migrações (-p) é `Direcional.Infrastructure` (onde vive o `AppDbContext`).
- Em Docker, a connection string usa o SQL Server do `docker-compose`; localmente valide `Direcional.Api/appsettings.json`.

### Fluxo recomendado (exemplo)
1) Logar e copiar o JWT
2) Liste os apartamentos: `GET /apartments`
3) Crie um cliente (se necessário): `POST /clients`
4) Crie uma reserva: `POST /reservations`
5) Confirme a venda: `POST /sales`

### Eventos e RabbitMQ
- Ao criar reserva, publica `ReservationCreated`
- Ao confirmar venda, publica `SaleConfirmed`

Como observar:
- UI do RabbitMQ → Queues: ver filas configuradas pelos consumers e taxas de mensagens
- Logs da API mostram consumo (por exemplo: `[ReservationCreated] ...`)

Opcional (inspeção manual de mensagens):
- É possível criar uma fila “audit” no RabbitMQ UI e bindar ao exchange do tipo de mensagem para reter mensagens sem consumidor, só para ver o teor das mensagens e verificar a conexão.

### Healthchecks
- (Base prontos para adicionar) Via ASP.NET HealthChecks e Docker. Pode-se expor `/health` se necessário.

### Execução local sem Docker (opcional)
1) dotnet 9 SDK instalado
2) Ajuste `Direcional.Api/appsettings.json` se necessário
3) Execute:
```bash
dotnet run --project Direcional.Api
```

### Testes (opcional)
- Estrutura preparada para adicionar xUnit/NUnit. Sugestão: cobrir login, CRUD básico e reserva/venda.

### Decisões técnicas
- .NET 9
- DDD/CQRS com MediatR
- EF Core SqlServer
- JWT para auth
- MassTransit + RabbitMQ para eventos
- Serilog + Swagger

### Troubleshooting
- Swagger não mostra endpoints: reinicie o container da API
- 401: confirme o header `Authorization: Bearer <token>` sem sufixos
- SQL Server indisponível: aguarde healthcheck e reinicie a API
- RabbitMQ sem mensagens: com consumidor ativo, mensagens são consumidas rapidamente; confira “Message rates”
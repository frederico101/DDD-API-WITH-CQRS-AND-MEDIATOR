## Direcional Imobiliária - API (.NET 9) com DDD, CQRS, MediatR e RabbitMQ

### Visão Geral
API de exemplo para o desafio técnico da Direcional Engenharia, construída com boas práticas: DDD/EDD, CQRS com MediatR, EF Core, JWT, Serilog, Swagger e RabbitMQ (MassTransit) para eventos de domínio.

Serviços via Docker Compose:
- SQL Server Express (banco DirecionalDb)
- RabbitMQ (management em `http://localhost:15672`)
- API em `http://localhost:8080`
- Frontend React em `http://localhost:5173`

### Arquitetura e Estrutura
- Camadas (DDD/CQRS):
  - `Direcional.Domain`: entidades, enums.
  - `Direcional.Application`: casos de uso (Commands/Queries via MediatR), validações (FluentValidation), contratos de contexto.
  - `Direcional.Infrastructure`: EF Core (DbContext, configurations, migrations), integração MassTransit/RabbitMQ, DI.
  - `Direcional.Api`: endpoints mínimos (Swagger, JWT, Serilog).
  - `Direcional.Tests`: testes unitários e de integração.
- Padrões:
  - CQRS com MediatR (Handlers por feature: Clients, Apartments, Reservations, Sales)
  - EDD: publicação de eventos `ReservationCreated`, `SaleConfirmed` no RabbitMQ
  - Mapeamentos EF Core por configuração (Fluent API)
  - ProblemDetails, HealthChecks prontos para extensão

### Tecnologias
- .NET 9, EF Core 9, MediatR, MassTransit, RabbitMQ, Serilog, Swagger, FluentValidation, Mapster, xUnit

### Requisitos
- Docker e Docker Compose instalados

### Subindo o projeto
```bash
docker compose up -d --build
```

- Acesse Swagger: `http://localhost:8080/swagger`
- Frontend React: `http://localhost:5173`
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

Importante: a chave JWT (HS256) deve ter pelo menos 256 bits (≥ 32 caracteres). Configure via:
- `Direcional.Api/appsettings.json` → `Jwt:Key`
- ou variável de ambiente Docker `Jwt__Key`.

### Interface Frontend (React)
O frontend React oferece uma interface intuitiva para gerenciar apartamentos, clientes, reservas e vendas.

#### Estados dos Botões de Reserva
Os apartamentos exibem diferentes estados visuais baseados em seu status:

- 🔵 **"Reservar"** - Apartamentos disponíveis (clicável)
- ⚫ **"Reservado"** - Apartamentos reservados (desabilitado, cinza)
- 🟢 **"Vendido"** - Apartamentos vendidos (desabilitado, verde)

Os botões mudam automaticamente de estado quando:
- Uma reserva é criada (disponível → reservado)
- Uma venda é confirmada (reservado → vendido)

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

### Testes (unitários e BDD)
- Ambiente de testes usa EF Core InMemory e MassTransit InMemory automaticamente quando `ASPNETCORE_ENVIRONMENT=Testing`.
- Rodar testes localmente (sem depender do SDK instalado) usando container .NET 9:
```bash
docker run --rm \
  -v "$PWD":/workspace -w /workspace \
  -e ASPNETCORE_ENVIRONMENT=Testing \
  mcr.microsoft.com/dotnet/sdk:9.0 dotnet test
```
- Cobertura (exemplos já inclusos):
  - Unit: geração de JWT, reserva publica evento e muda status, venda confirma e muda status
  - Integração (BDD-like): login → criar apartamento → criar reserva; login → criar apartamento → confirmar venda

### Healthchecks
- (Base prontos para adicionar) Via ASP.NET HealthChecks e Docker. Pode-se expor `/health` se necessário.

### Execução local sem Docker (opcional)
1) dotnet 9 SDK instalado
2) Ajuste `Direcional.Api/appsettings.json` se necessário
3) Execute:
```bash
dotnet run --project Direcional.Api
```

### Exemplos de requisições (curl)
```bash
# Login
curl -s -X POST http://localhost:8080/auth/login \
  -H 'Content-Type: application/json' \
  -d '{"username":"admin","password":"admin123"}'

# Listar apartamentos (autenticado)
TOKEN="<JWT>"
curl -s "http://localhost:8080/apartments?page=1&pageSize=5" \
  -H "Authorization: Bearer $TOKEN"

# Criar cliente
curl -s -X POST http://localhost:8080/clients \
  -H 'Content-Type: application/json' -H "Authorization: Bearer $TOKEN" \
  -d '{"name":"John","email":"john@example.com","document":"123","phone":"+55 11 99999-9999"}'

# Criar apartamento
curl -s -X POST http://localhost:8080/apartments \
  -H 'Content-Type: application/json' -H "Authorization: Bearer $TOKEN" \
  -d '{"code":"C-301","block":"C","floor":3,"number":301,"price":420000}'

# Criar reserva
curl -s -X POST http://localhost:8080/reservations \
  -H 'Content-Type: application/json' -H "Authorization: Bearer $TOKEN" \
  -d '{"clientId":"<clientId>","apartmentId":"<apartmentId>","expiresHours":24}'

# Confirmar venda
curl -s -X POST http://localhost:8080/sales \
  -H 'Content-Type: application/json' -H "Authorization: Bearer $TOKEN" \
  -d '{"clientId":"<clientId>","apartmentId":"<apartmentId>","downPayment":10000,"totalPrice":420000}'
```

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
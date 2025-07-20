# Analytics API - Exemplos de Uso

## Endpoints Disponíveis

### 1. Dashboard Completo
```
GET /api/Analytics/dashboard
```

**Parâmetros opcionais:**
- `startDate`: Data de início (formato ISO: 2025-07-01T00:00:00Z)
- `endDate`: Data de fim (formato ISO: 2025-07-31T23:59:59Z)
- `status`: Filtrar por status específico (pending, preparation, ready, delivered)

**Exemplo de resposta:**
```json
{
  "totalOrders": 5,
  "totalRevenue": 206.60,
  "averageTicket": 41.32,
  "ordersInPreparation": 1,
  "ordersByStatus": {
    "ready": 3,
    "preparation": 1,
    "delivered": 1
  },
  "topProducts": [
    {
      "productId": "a1239ede-2938-40ee-85ce-962d56e227c3",
      "productName": "X-Giga Burger",
      "quantitySold": 4,
      "revenue": 200.00
    },
    {
      "productId": "c8015ba8-7b05-4484-b4a4-2317cf8228ed",
      "productName": "Coca Cola 2L",
      "quantitySold": 3,
      "revenue": 30.00
    }
  ]
}
```

### 2. Distribuição por Status
```
GET /api/Analytics/orders-by-status
```

**Exemplo de resposta:**
```json
[
  {
    "status": "ready",
    "count": 3,
    "percentage": 60.00
  },
  {
    "status": "preparation",
    "count": 1,
    "percentage": 20.00
  },
  {
    "status": "delivered",
    "count": 1,
    "percentage": 20.00
  }
]
```

### 3. Produtos Mais Vendidos
```
GET /api/Analytics/top-products?top=5
```

**Exemplo de resposta:**
```json
[
  {
    "productId": "a1239ede-2938-40ee-85ce-962d56e227c3",
    "productName": "X-Giga Burger",
    "quantitySold": 4,
    "revenue": 200.00
  },
  {
    "productId": "c8015ba8-7b05-4484-b4a4-2317cf8228ed",
    "productName": "Coca Cola 2L",
    "quantitySold": 3,
    "revenue": 30.00
  }
]
```

### 4. Health Check
```
GET /api/Analytics/health
```

**Exemplo de resposta:**
```json
{
  "status": "healthy",
  "service": "analytics",
  "timestamp": "2025-07-20T10:30:00.000Z"
}
```

## Exemplos de Filtros

### Filtrar por período:
```
GET /api/Analytics/dashboard?startDate=2025-07-01&endDate=2025-07-31
```

### Filtrar por status:
```
GET /api/Analytics/dashboard?status=ready
```

### Combinar filtros:
```
GET /api/Analytics/dashboard?startDate=2025-07-01&endDate=2025-07-31&status=preparation
```

## Mapeamento para o Dashboard

Os dados retornados podem ser usados diretamente para popular o dashboard:

- **Total de Pedidos**: `totalOrders`
- **Receita Total**: `totalRevenue` (formato: R$ 206.60)
- **Ticket Médio**: `averageTicket` (formato: R$ 41.32)
- **Em Preparo**: `ordersInPreparation`
- **Gráfico de Pizza**: `ordersByStatus`
- **Gráfico de Barras**: `topProducts`

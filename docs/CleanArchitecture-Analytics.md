# Estrutura Clean Architecture - Analytics

## Resumo da Implementação

A funcionalidade de Analytics foi implementada seguindo os princípios da Clean Architecture, com separação clara de responsabilidades entre as camadas:

## 📁 Estrutura de Arquivos

### Domain Layer (`FastTechFoodsKitchen.Domain`)
- **Entities/Order.cs**: Entidade de domínio representando um pedido

### Application Layer (`FastTechFoodsKitchen.Application`)
- **Interfaces/IAnalyticsService.cs**: Interface do serviço de domínio
- **Interfaces/IAnalyticsRepository.cs**: Interface do repositório (abstração para infraestrutura)
- **Interfaces/IApplicationDbContext.cs**: Interface para abstrair o contexto de dados
- **DTOs/Analytics/**: DTOs para transferência de dados
  - `DashboardAnalyticsDto.cs`
  - `ProductSalesDto.cs`
  - `OrderStatusDistributionDto.cs`
  - `AnalyticsFilterDto.cs`
- **Services/AnalyticsService.cs**: Implementação da lógica de negócio

### Infrastructure Layer (`FastTechFoodsKitchen.Infrastructure`)
- **Repositories/AnalyticsRepository.cs**: Implementação do repositório com acesso ao MongoDB
- **Context/ApplicationDbContext.cs**: Contexto do MongoDB (implementa IApplicationDbContext)

### API Layer (`FastTechFoodsKitchen.Api`)
- **Controllers/AnalyticsController.cs**: Controller REST API
- **Program.cs**: Configuração de DI

## 🔄 Fluxo de Dependências

```
API → Application → Domain
  ↘     ↓
   Infrastructure
```

### Princípios Aplicados:

1. **Dependency Inversion**: Application depende de interfaces, não de implementações
2. **Single Responsibility**: Cada classe tem uma responsabilidade específica
3. **Interface Segregation**: Interfaces específicas para cada contexto
4. **Separation of Concerns**: Lógica de negócio separada de infraestrutura

## 🔧 Configuração de Injeção de Dependência

```csharp
// MongoDB Context
builder.Services.AddSingleton<ApplicationDbContext>();
builder.Services.AddSingleton<IApplicationDbContext>(provider => 
    provider.GetRequiredService<ApplicationDbContext>());

// Repositories (Infrastructure)
builder.Services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();

// Services (Application)
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
```

## 🚀 Endpoints Disponíveis

1. **GET /api/Analytics/dashboard** - Dashboard completo
2. **GET /api/Analytics/orders-by-status** - Distribuição por status
3. **GET /api/Analytics/top-products** - Produtos mais vendidos
4. **GET /api/Analytics/health** - Health check

## ✅ Benefícios da Arquitetura

1. **Testabilidade**: Interfaces permitem mock/stub fácil para testes
2. **Flexibilidade**: Pode trocar MongoDB por outro banco facilmente
3. **Manutenibilidade**: Código organizado e com responsabilidades claras
4. **Reutilização**: Serviços podem ser usados em outros contextos
5. **Evolução**: Novas funcionalidades podem ser adicionadas sem quebrar o existente

## 🔄 Padrões Implementados

- **Repository Pattern**: Abstração do acesso a dados
- **Dependency Injection**: Inversão de controle
- **DTO Pattern**: Transferência de dados entre camadas
- **Service Layer Pattern**: Encapsulamento da lógica de negócio
- **Interface Segregation**: Interfaces específicas e coesas

Esta estrutura garante que o código seja testável, flexível e mantenha a separação de responsabilidades seguindo os princípios SOLID.

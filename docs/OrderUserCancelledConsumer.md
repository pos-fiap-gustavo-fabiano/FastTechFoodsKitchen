# Consumer para Order User Cancelled

## 📋 **Implementação Realizada**

Foi criado um consumer seguindo o padrão existente no projeto para processar mensagens da fila `order.user.cancelled.queue`.

## 🗂️ **Arquivos Criados**

### 1. Interface do Handler
**Arquivo**: `IOrderUserCancelledMessageHandler.cs`
```csharp
public interface IOrderUserCancelledMessageHandler
{
    Task HandleAsync(OrderCancelledMessage orderUserCancelledMessage);
}
```

### 2. Implementação do Handler
**Arquivo**: `OrderUserCancelledMessageHandler.cs`
- Processa mensagens de cancelamento iniciado pelo usuário
- Usa o `IKitchenOrderService.CancelOrderAsync()` para cancelar o pedido na cozinha
- Inclui telemetria com ActivitySource
- Log completo das operações

### 3. Consumer Background Service
**Arquivo**: `OrderUserCancelledMessageConsumer.cs`
- Background service que escuta a fila `order.user.cancelled.queue`
- Configuração de fila durável
- Acknowledgment manual das mensagens
- Tratamento de erro com NACK

## ⚙️ **Configuração**

### Program.cs
```csharp
// Registra o handler e consumer
builder.Services.AddScoped<IOrderUserCancelledMessageHandler, OrderUserCancelledMessageHandler>();
builder.Services.AddHostedService<OrderUserCancelledMessageConsumer>();
```

## 🔄 **Fluxo de Processamento**

1. **Recepção**: Consumer escuta mensagens na fila `order.user.cancelled.queue`
2. **Deserialização**: Converte JSON para `OrderCancelledMessage`
3. **Processamento**: Handler chama `CancelOrderAsync()` com:
   - OrderId da mensagem
   - CancelledBy: "Sistema"  
   - Reason: "Cancelado pelo usuário"
   - Notes: "Cancelamento originado do sistema de pedidos"
4. **Acknowledgment**: Confirma processamento ou rejeita em caso de erro

## 📊 **Telemetria e Logs**

### Activity Tracing
- Source: `FastTechFoodsKitchen.OrderUserCancelledMessageHandler`
- Tags: `order.id`, `operation: order.user.cancelled`

### Logs Estruturados
```
Processing OrderUserCancelled for Order {OrderId}
OrderUserCancelled processed successfully for Order {OrderId}
Error processing OrderUserCancelled for Order {OrderId}
```

## 🚀 **Características**

- ✅ **Durabilidade**: Fila configurada como durável
- ✅ **Confiabilidade**: Acknowledgment manual
- ✅ **Observabilidade**: Logs e telemetria completos
- ✅ **Tratamento de Erro**: NACK em caso de falha
- ✅ **Padrão Consistente**: Segue mesmo padrão do OrderCreatedConsumer
- ✅ **Injeção de Dependência**: Usa scoped services corretamente

## 🔧 **Uso**

O consumer será iniciado automaticamente quando a aplicação subir e ficará escutando continuamente a fila `order.user.cancelled.queue`. Quando uma mensagem chegar:

1. Deserializa a mensagem
2. Cancela o pedido na cozinha via `KitchenOrderService`
3. Publica evento de cancelamento se necessário
4. Confirma o processamento

Este consumer permite que a cozinha seja notificada automaticamente quando um usuário cancela um pedido no sistema principal.

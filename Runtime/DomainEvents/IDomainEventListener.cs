using Cysharp.Threading.Tasks;

public interface IDomainEventListener
{
}

public interface IDomainEventListener<T> : IDomainEventListener where T : DomainEvent
{
    UniTask OnEventRaisedAsync(T domainEvent);
}

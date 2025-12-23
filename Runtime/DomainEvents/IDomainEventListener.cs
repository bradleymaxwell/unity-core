public interface IDomainEventListener
{
}

public interface IDomainEventListener<T> : IDomainEventListener where T : DomainEvent
{
    void OnEventRaised(T domainEvent);
}

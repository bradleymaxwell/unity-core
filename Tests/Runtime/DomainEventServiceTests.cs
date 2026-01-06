using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class DomainEventServiceTests
{
    [Test]
    public void RegisterAddsNewListenerWhenNotAlreadyRegistered()
    {
        // Arrange
        var sut = new DomainEventService();
        var listener = new TestDomainEventListener();
        
        // Act
        sut.Register(listener);
        
        // Assert
        Assert.IsTrue(sut.IsRegistered(listener));
    }
    
    [Test]
    public void RegisterDoesNotAddListenerAgainWhenAlreadyRegistered()
    {
        // Arrange
        var sut = new DomainEventService();
        var listener = new TestDomainEventListener();
        sut.Register(listener);
        var countBefore = sut.GetListenerCount<TestDomainEvent>();
        
        // Act
        sut.Register(listener);
        var countAfter = sut.GetListenerCount<TestDomainEvent>();
        
        // Assert
        Assert.AreEqual(countBefore, countAfter);
    }
    
    [Test]
    public void UnregisterRemovesListenerWhenListenerRegistered()
    {
        // Arrange
        var sut = new DomainEventService();
        var listener = new TestDomainEventListener();
        sut.Register(listener);
        
        // Act
        sut.Unregister(listener);
        
        // Assert
        Assert.IsFalse(sut.IsRegistered(listener));
    }

    [UnityTest]
    public IEnumerator RaiseTriggersAllListenersWhenOneListenerThrowsException() => UniTask.ToCoroutine(async () =>
    {
        // Arrange
        var sut = new DomainEventService();
        var listeners = new List<TestDomainEventListener>
        {
            new(),
            new BreakingTestDomainEventListener(),
            new(),
            new()
        };
        var domainEvent = new TestDomainEvent();

        foreach (var listener in listeners)
        {
            sut.Register(listener);
        }

        LogAssert.Expect(LogType.Error, new Regex("Error encountered when raising"));

        // Act
        await sut.RaiseAsync(domainEvent);

        // Assert
        foreach (var listener in listeners)
        {
            Assert.IsTrue(listener.WasCalled);
        }
    });

    private class TestDomainEventListener : IDomainEventListener<TestDomainEvent>
    {
        public bool WasCalled { get; private set; }
        
        public virtual UniTask OnEventRaisedAsync(TestDomainEvent domainEvent)
        {
            WasCalled = true;
            return UniTask.CompletedTask;
        }
    }

    private class BreakingTestDomainEventListener : TestDomainEventListener
    {
        public override async UniTask OnEventRaisedAsync(TestDomainEvent domainEvent)
        {
            await base.OnEventRaisedAsync(domainEvent);
            throw new Exception();
        }
    }
    
    private class TestDomainEvent : DomainEvent
    {
    }
}

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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
    
    [Test]
    public void RaiseTriggersAllListenersWhenOneListenerThrowsException()
    {
        // Arrange
        var sut = new DomainEventService();
        var listeners = new List<TestDomainEventListener>
        {
            new(),
            new BreakingTestDomainEventListener(),
            new (),
            new ()
        };
        var domainEvent = new TestDomainEvent();

        foreach (var listener in listeners)
        {
            sut.Register(listener);
        }
        
        LogAssert.Expect(LogType.Error,new Regex("Error encountered when raising"));

        // Act
        sut.Raise(domainEvent);

        // Assert
        foreach (var listener in listeners)
        {
            Assert.IsTrue(listener.WasCalled);
        }
    }

    private class TestDomainEventListener : IDomainEventListener<TestDomainEvent>
    {
        public bool WasCalled { get; private set; }
        
        public virtual void OnEventRaised(TestDomainEvent domainEvent)
        {
            WasCalled = true;
        }
    }

    private class BreakingTestDomainEventListener : TestDomainEventListener
    {
        public override void OnEventRaised(TestDomainEvent domainEvent)
        {
            base.OnEventRaised(domainEvent);
            throw new Exception();
        }
    }
    
    private class TestDomainEvent : DomainEvent
    {
    }
}

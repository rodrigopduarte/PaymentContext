using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PaymentContext.Domain.Entities;
using PaymentContext.Domain.Enums;
using PaymentContext.Domain.ValueObjects;

namespace PaymentContext.Entities.Tests
{
    [TestClass]
    public class StudentTests
    {
        private readonly Student _student;
        private readonly Subscription _subscription;
        private readonly Name _name;
        private readonly Document _document;
        private readonly Email _email;
        private readonly Address _address;

        public StudentTests()
        {
            _name = new Name("Bruce", "Wayne");
            _document = new Document("12345678901", EDocumentType.CPF);
            _email = new Email("batman@dc.com");
            _address = new Address("Boulevard", "1-10", "Center", "Gothan", "LA", "EUA", "10PO879");
            _student = new Student(_name, _document, _email);
            _subscription = new Subscription(null);
        }

        [TestMethod]
        public void ShouldReturnErrorWhenHadActiveSubscription()
        {
            var payment = new PayPalPayment("998899889988", DateTime.Now, DateTime.Now.AddDays(5), 80, 80, "Wayne Corp", _document, _address, _email);
            _subscription.AddPayment(payment);
            _student.AddSubscription(_subscription);
            _student.AddSubscription(_subscription);

            Assert.IsTrue(_student.Invalid);
        }

        [TestMethod]
        public void ShouldReturnErrorWhenHadSubscriptionHasNoPayment()
        {
            _student.AddSubscription(_subscription);
            Assert.IsTrue(_student.Invalid);
        }

        [TestMethod]
        public void ShouldReturnSuccessWhenHadNoActiveSubscription()
        {
            var payment = new PayPalPayment("998899889988", DateTime.Now, DateTime.Now.AddDays(5), 80, 80, "Wayne Corp", _document, _address, _email);
            _subscription.AddPayment(payment);
            _student.AddSubscription(_subscription);
            Assert.IsTrue(_student.Valid);
        }
    }
}

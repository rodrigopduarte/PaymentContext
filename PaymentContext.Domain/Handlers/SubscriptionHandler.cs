using System;
using Flunt.Notifications;
using Flunt.Validations;
using PaymentContext.Domain.Commands;
using PaymentContext.Domain.Entities;
using PaymentContext.Domain.Enums;
using PaymentContext.Domain.Repositories;
using PaymentContext.Domain.Services;
using PaymentContext.Domain.ValueObjects;
using PaymentContext.Shared.Commands;
using PaymentContext.Shared.Handlers;

namespace PaymentContext.Domain.Handlers
{
    public class SubscriptionHandler : Notifiable,
        IHandler<CreateBoletoSubscriptionCommand>
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IEmailService _emailService;

        public SubscriptionHandler(IStudentRepository studentRepository,
            IEmailService emailService)
        {
            _studentRepository = studentRepository;
            _emailService = emailService;
        }

        public ICommandResult Handle(CreateBoletoSubscriptionCommand command)
        {
            // Fail fast validations
            command.Validate();
            if (command.Invalid) 
            {
                AddNotifications(command);
                return new CommandResult(false, "Não foi possível realizar a sua assinatura");
            }
            
            // Verificar se o documento já está cadastrado
            // Verificar se o email já está cadastrado
            AddNotifications(new Contract()
                .Requires()
                .IsTrue(_studentRepository.DocumentExists(command.Document), "Subscription.Document", "Este CPF já está em uso")
                .IsTrue(_studentRepository.EmailExists(command.Email), "Subscription.Email", "Este e-mail já está em uso")
            );
            
            // Gerar os VOs
            var name = new Name(command.FirstName, command.LastName);
            var document = new Document(command.Document, EDocumentType.CPF);
            var email = new Email(command.Email);
            var address = new Address(command.Street, command.Number, command.Neighborhood, command.City, command.State, command.Country, command.ZipCode);
            var payerDocument = new Document(command.PayerDocument, command.PayerDocumentType);

            // Gerar as entidade
            var student = new Student(name, document, email);
            var subscription = new Subscription(DateTime.Now.AddMonths(1));
            var payment = new BoletoPayment(command.BarCode, command.BoletoNumber, command.PaidDate, command.ExpireDate, command.Total, command.TotalPaid, command.Payer, payerDocument, address, email);

            // Relacionamentos
            subscription.AddPayment(payment);
            student.AddSubscription(subscription);

            // Aplicar as validações
            AddNotifications(name, document, payerDocument, email, address, student, subscription, payment);

            // Checar as notificações
            if (Invalid)
                return new CommandResult(false, "Não foi possível realizar sua assinatura");

            // Salvar as informações
            _studentRepository.CreateSubscription(student);

            // Enviar E-mail de boas vindas
            _emailService.Send(student.Name.ToString(), student.Email.Address, "bem vindo ao balta.io", "Sua assinatura foi criada com sucesso");

            // Retornar informações
            return new CommandResult(true, "Assinatura realizada com sucesso");
        }
    }
}
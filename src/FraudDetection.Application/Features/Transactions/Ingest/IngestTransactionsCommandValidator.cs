using FluentValidation;
using FraudDetection.Domain.Enums;

namespace FraudDetection.Application.Features.Transactions.Ingest;

public sealed class IngestTransactionsCommandValidator : AbstractValidator<IngestTransactionsCommand>
{
    public IngestTransactionsCommandValidator()
    {
        RuleFor(command => command.Transactions)
            .NotEmpty()
            .WithMessage("At least one transaction is required.")
            .Must(transactions => transactions.Count <= 500)
            .WithMessage("A batch cannot contain more than 500 transactions.");

        RuleForEach(command => command.Transactions)
            .SetValidator(new TransactionIngestionItemValidator());
    }
}

internal sealed class TransactionIngestionItemValidator : AbstractValidator<TransactionIngestionItem>
{
    public TransactionIngestionItemValidator()
    {
        RuleFor(transaction => transaction.EventType)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(transaction => transaction.TransactionId)
            .NotEmpty();

        RuleFor(transaction => transaction.CustomerId)
            .NotEmpty();

        RuleFor(transaction => transaction.Points)
            .NotEqual(0);

        RuleFor(transaction => transaction.Type)
            .Must(type => Enum.IsDefined(type))
            .WithMessage("Transaction type is invalid.");

        RuleFor(transaction => transaction.Description)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(transaction => transaction.ReferenceId)
            .MaximumLength(200)
            .When(transaction => transaction.ReferenceId is not null);

        RuleFor(transaction => transaction.CreatedAt)
            .NotEqual(default(DateTime));
    }
}

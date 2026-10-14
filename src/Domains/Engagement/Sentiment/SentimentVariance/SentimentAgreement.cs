namespace Acme.Workforce.Engagement.Sentiment.SentimentVariance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a sentiment agreement is in its life cycle.</summary>
public enum SentimentAgreementStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A sentiment agreement. Its code is unique within a tenant.</summary>
public sealed class SentimentAgreement(string code, string name, string jobCode, string externalRef, string summary, int updatedAt, string title, SentimentAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string JobCode { get; set; } = jobCode;
    public string ExternalRef { get; set; } = externalRef;
    public string Summary { get; set; } = summary;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Title { get; set; } = title;
    public SentimentAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a sentiment agreement in status created.</summary>
    public static SentimentAgreement NewSentimentAgreement(string code, string name)
    {
        return new SentimentAgreement(code, name, "", "", "", 0, "", SentimentAgreementStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SentimentAgreementStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SentimentAgreementStatus next)
    {
        switch (Status)
        {
            case SentimentAgreementStatus.Created:
                switch (next)
                {
                    case SentimentAgreementStatus.Published:
                    case SentimentAgreementStatus.Retired:
                        return true;
                }
                return false;
            case SentimentAgreementStatus.Validated:
                return next == SentimentAgreementStatus.Published || next == SentimentAgreementStatus.Retired;
            case SentimentAgreementStatus.Published:
                switch (next)
                {
                    case SentimentAgreementStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SentimentAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"sentiment agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double MeasuredValue()
    {
        return (double)UpdatedAt;
    }

    public SentimentAgreement Copy()
    {
        return new SentimentAgreement(Code, Name, JobCode, ExternalRef, Summary, UpdatedAt, Title, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}

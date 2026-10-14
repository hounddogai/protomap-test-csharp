namespace Acme.Workforce.Engagement.Recognition.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a recognition agreement is in its life cycle.</summary>
public enum RecognitionAgreementStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a recognition agreement and its current status.</summary>
public sealed class RecognitionAgreement(string code, string label, double threshold, double minimum, string locale, string jobCode, RecognitionAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Threshold { get; set; } = threshold;
    public double Minimum { get; set; } = minimum;
    public string Locale { get; set; } = locale;
    public string JobCode { get; set; } = jobCode;
    public RecognitionAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a recognition agreement in status draft.</summary>
    public static RecognitionAgreement NewRecognitionAgreement(string code, string label)
    {
        return new RecognitionAgreement(code, label, 0.0, 0.0, "", "", RecognitionAgreementStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RecognitionAgreementStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RecognitionAgreementStatus next)
    {
        switch (Status)
        {
            case RecognitionAgreementStatus.Draft:
                switch (next)
                {
                    case RecognitionAgreementStatus.Active:
                    case RecognitionAgreementStatus.Closed:
                        return true;
                }
                return false;
            case RecognitionAgreementStatus.Pending:
                switch (next)
                {
                    case RecognitionAgreementStatus.Active:
                    case RecognitionAgreementStatus.Suspended:
                        return true;
                }
                return false;
            case RecognitionAgreementStatus.Active:
                switch (next)
                {
                    case RecognitionAgreementStatus.Suspended:
                    case RecognitionAgreementStatus.Closed:
                        return true;
                }
                return false;
            case RecognitionAgreementStatus.Suspended:
                return next == RecognitionAgreementStatus.Closed;
        }
        return false;
    }

    public void MoveTo(RecognitionAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"recognition agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"recognition agreement"} {Code}";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Minimum - Threshold, 2);
    }

    public RecognitionAgreement Copy()
    {
        return new RecognitionAgreement(Code, Label, Threshold, Minimum, Locale, JobCode, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}

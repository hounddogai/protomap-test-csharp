namespace Acme.Workforce.Engagement.Recognition.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a recognition policy is in its life cycle.</summary>
public enum RecognitionPolicyStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A recognition policy. Its code is unique within a tenant.</summary>
public sealed class RecognitionPolicy(string code, string title, double margin, double score, double ratio, double amount, RecognitionPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Margin { get; set; } = margin;
    public double Score { get; set; } = score;
    public double Ratio { get; set; } = ratio;
    public double Amount { get; set; } = amount;
    public RecognitionPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a recognition policy in status created.</summary>
    public static RecognitionPolicy NewRecognitionPolicy(string code, string title)
    {
        return new RecognitionPolicy(code, title, 0.0, 0.0, 0.0, 0.0, RecognitionPolicyStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RecognitionPolicyStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RecognitionPolicyStatus next)
    {
        switch (Status)
        {
            case RecognitionPolicyStatus.Created:
                return next == RecognitionPolicyStatus.Retired;
            case RecognitionPolicyStatus.Validated:
                return next == RecognitionPolicyStatus.Published;
            case RecognitionPolicyStatus.Published:
                switch (next)
                {
                    case RecognitionPolicyStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RecognitionPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"recognition policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double EffectiveValue()
    {
        return Math.Round(Margin * Ratio, 2);
    }

    public RecognitionPolicy Copy()
    {
        return new RecognitionPolicy(Code, Title, Margin, Score, Ratio, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}

namespace Acme.Workforce.Compliance.TrainingDue.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a training due budget is in its life cycle.</summary>
public enum TrainingDueBudgetStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a training due budget and its current status.</summary>
public sealed class TrainingDueBudget(string code, string name, int endsAt, string externalRef, double fte, string badge, string locale, string jobCode, string timezone, string segment, TrainingDueBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int EndsAt { get; set; } = endsAt;
    public string ExternalRef { get; set; } = externalRef;
    public double Fte { get; set; } = fte;
    public string Badge { get; set; } = badge;
    public string Locale { get; set; } = locale;
    public string JobCode { get; set; } = jobCode;
    public string Timezone { get; set; } = timezone;
    public string Segment { get; set; } = segment;
    public TrainingDueBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a training due budget in status draft.</summary>
    public static TrainingDueBudget NewTrainingDueBudget(string code, string name)
    {
        return new TrainingDueBudget(code, name, 0, "", 0.0, "", "", "", "", "", TrainingDueBudgetStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TrainingDueBudgetStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TrainingDueBudgetStatus next)
    {
        switch (Status)
        {
            case TrainingDueBudgetStatus.Draft:
                return next == TrainingDueBudgetStatus.Pending || next == TrainingDueBudgetStatus.Closed;
            case TrainingDueBudgetStatus.Pending:
                return next == TrainingDueBudgetStatus.Active;
            case TrainingDueBudgetStatus.Active:
                return next == TrainingDueBudgetStatus.Closed;
            case TrainingDueBudgetStatus.Suspended:
                return next == TrainingDueBudgetStatus.Closed;
        }
        return false;
    }

    public void MoveTo(TrainingDueBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"training due budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"training due budget"} {Code}";
    }

    public double EffectiveValue()
    {
        return Math.Round((double)EndsAt - Fte, 2);
    }

    public TrainingDueBudget Copy()
    {
        return new TrainingDueBudget(
            Code,
            Name,
            EndsAt,
            ExternalRef,
            Fte,
            Badge,
            Locale,
            JobCode,
            Timezone,
            Segment,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}

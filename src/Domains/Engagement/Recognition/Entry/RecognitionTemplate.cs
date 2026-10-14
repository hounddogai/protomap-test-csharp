namespace Acme.Workforce.Engagement.Recognition.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a recognition template is in its life cycle.</summary>
public enum RecognitionTemplateStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a recognition template and its current status.</summary>
public sealed class RecognitionTemplate(string code, string name, double fte, string managerId, double score, bool active, string team, double ratio, string currency, bool archived, double hours, RecognitionTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Fte { get; set; } = fte;
    public string ManagerId { get; set; } = managerId;
    public double Score { get; set; } = score;
    public bool Active { get; set; } = active;
    public string Team { get; set; } = team;
    public double Ratio { get; set; } = ratio;
    public string Currency { get; set; } = currency;
    public bool Archived { get; set; } = archived;
    public double Hours { get; set; } = hours;
    public RecognitionTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a recognition template in status proposed.</summary>
    public static RecognitionTemplate NewRecognitionTemplate(string code, string name)
    {
        return new RecognitionTemplate(
            code,
            name,
            0.0,
            "",
            0.0,
            false,
            "",
            0.0,
            "",
            false,
            0.0,
            RecognitionTemplateStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RecognitionTemplateStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RecognitionTemplateStatus next)
    {
        switch (Status)
        {
            case RecognitionTemplateStatus.Proposed:
                return next == RecognitionTemplateStatus.Approved || next == RecognitionTemplateStatus.Rejected;
            case RecognitionTemplateStatus.Approved:
                return next == RecognitionTemplateStatus.Withdrawn;
            case RecognitionTemplateStatus.Rejected:
                switch (next)
                {
                    case RecognitionTemplateStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RecognitionTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"recognition template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double EffectiveValue()
    {
        return Math.Round(Ratio * Score, 2);
    }

    public RecognitionTemplate Copy()
    {
        return new RecognitionTemplate(
            Code,
            Name,
            Fte,
            ManagerId,
            Score,
            Active,
            Team,
            Ratio,
            Currency,
            Archived,
            Hours,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}

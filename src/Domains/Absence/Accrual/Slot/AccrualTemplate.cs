namespace Acme.Workforce.Absence.Accrual.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a accrual template is in its life cycle.</summary>
public enum AccrualTemplateStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a accrual template and its current status.</summary>
public sealed class AccrualTemplate(string code, string name, double rate, double ratio, List<string> tags, string segment, bool pinned, string unit, AccrualTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Rate { get; set; } = rate;
    public double Ratio { get; set; } = ratio;
    public List<string> Tags { get; set; } = tags;
    public string Segment { get; set; } = segment;
    public bool Pinned { get; set; } = pinned;
    public string Unit { get; set; } = unit;
    public AccrualTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a accrual template in status draft.</summary>
    public static AccrualTemplate NewAccrualTemplate(string code, string name)
    {
        return new AccrualTemplate(
            code,
            name,
            0.0,
            0.0,
            new List<string>(),
            "",
            false,
            "",
            AccrualTemplateStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccrualTemplateStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccrualTemplateStatus next)
    {
        switch (Status)
        {
            case AccrualTemplateStatus.Draft:
                switch (next)
                {
                    case AccrualTemplateStatus.Suspended:
                    case AccrualTemplateStatus.Closed:
                        return true;
                }
                return false;
            case AccrualTemplateStatus.Pending:
                switch (next)
                {
                    case AccrualTemplateStatus.Active:
                    case AccrualTemplateStatus.Suspended:
                        return true;
                }
                return false;
            case AccrualTemplateStatus.Active:
                switch (next)
                {
                    case AccrualTemplateStatus.Suspended:
                    case AccrualTemplateStatus.Closed:
                        return true;
                }
                return false;
            case AccrualTemplateStatus.Suspended:
                switch (next)
                {
                    case AccrualTemplateStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AccrualTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"accrual template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double EffectiveTotal()
    {
        return Math.Round(Rate * Ratio, 2);
    }

    public AccrualTemplate Copy()
    {
        return new AccrualTemplate(Code, Name, Rate, Ratio, Tags, Segment, Pinned, Unit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}

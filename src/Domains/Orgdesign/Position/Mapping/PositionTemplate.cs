namespace Acme.Workforce.Orgdesign.Position.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a position template is in its life cycle.</summary>
public enum PositionTemplateStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a position template and its current status.</summary>
public sealed class PositionTemplate(string code, string name, double score, double ceiling, int graceDays, string country, double discount, string label, PositionTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Score { get; set; } = score;
    public double Ceiling { get; set; } = ceiling;
    public int GraceDays { get; set; } = graceDays;
    public string Country { get; set; } = country;
    public double Discount { get; set; } = discount;
    public string Label { get; set; } = label;
    public PositionTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a position template in status open.</summary>
    public static PositionTemplate NewPositionTemplate(string code, string name)
    {
        return new PositionTemplate(code, name, 0.0, 0.0, 0, "", 0.0, "", PositionTemplateStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PositionTemplateStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PositionTemplateStatus next)
    {
        switch (Status)
        {
            case PositionTemplateStatus.Open:
                switch (next)
                {
                    case PositionTemplateStatus.Reopened:
                    case PositionTemplateStatus.Archived:
                        return true;
                }
                return false;
            case PositionTemplateStatus.InReview:
                switch (next)
                {
                    case PositionTemplateStatus.Resolved:
                    case PositionTemplateStatus.Reopened:
                        return true;
                }
                return false;
            case PositionTemplateStatus.Resolved:
                switch (next)
                {
                    case PositionTemplateStatus.Reopened:
                    case PositionTemplateStatus.Archived:
                        return true;
                }
                return false;
            case PositionTemplateStatus.Reopened:
                switch (next)
                {
                    case PositionTemplateStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PositionTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"position template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"position template"} {Code}";
    }

    public double AdjustedTotal()
    {
        return Math.Round(Ceiling - Discount, 2);
    }

    public PositionTemplate Copy()
    {
        return new PositionTemplate(Code, Name, Score, Ceiling, GraceDays, Country, Discount, Label, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}

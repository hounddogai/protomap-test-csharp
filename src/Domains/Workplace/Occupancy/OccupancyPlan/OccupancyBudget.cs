namespace Acme.Workforce.Workplace.Occupancy.OccupancyPlan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a occupancy budget is in its life cycle.</summary>
public enum OccupancyBudgetStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A occupancy budget as the occupancy team keeps it.</summary>
public sealed class OccupancyBudget(string code, string label, Dictionary<string, string> attributes, string ownerId, string notes, string phone, double fte, OccupancyBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string OwnerId { get; set; } = ownerId;
    public string Notes { get; set; } = notes;
    public string Phone { get; set; } = phone;
    public double Fte { get; set; } = fte;
    public OccupancyBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a occupancy budget in status created.</summary>
    public static OccupancyBudget NewOccupancyBudget(string code, string label)
    {
        return new OccupancyBudget(
            code,
            label,
            new Dictionary<string, string>(),
            "",
            "",
            "",
            0.0,
            OccupancyBudgetStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OccupancyBudgetStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OccupancyBudgetStatus next)
    {
        switch (Status)
        {
            case OccupancyBudgetStatus.Created:
                switch (next)
                {
                    case OccupancyBudgetStatus.Published:
                    case OccupancyBudgetStatus.Retired:
                        return true;
                }
                return false;
            case OccupancyBudgetStatus.Validated:
                switch (next)
                {
                    case OccupancyBudgetStatus.Published:
                        return true;
                }
                return false;
            case OccupancyBudgetStatus.Published:
                switch (next)
                {
                    case OccupancyBudgetStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(OccupancyBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"occupancy budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double MeasuredValue()
    {
        return Fte;
    }

    public OccupancyBudget Copy()
    {
        return new OccupancyBudget(Code, Label, Attributes, OwnerId, Notes, Phone, Fte, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}

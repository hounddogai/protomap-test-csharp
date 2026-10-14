namespace Acme.Workforce.Workplace.Desk.DeskBudget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default desk budget is in its life cycle.</summary>
public enum DefaultDeskBudgetStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One default desk budget, with what the planners need to act on it.</summary>
public sealed class DefaultDeskBudget(string code, string title, bool active, double rating, double hours, string managerId, Dictionary<string, string> attributes, string checksum, int capacity, string summary, double budget, DefaultDeskBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Active { get; set; } = active;
    public double Rating { get; set; } = rating;
    public double Hours { get; set; } = hours;
    public string ManagerId { get; set; } = managerId;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Checksum { get; set; } = checksum;
    public int Capacity { get; set; } = capacity;
    public string Summary { get; set; } = summary;
    public double Budget { get; set; } = budget;
    public DefaultDeskBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a default desk budget in status created.</summary>
    public static DefaultDeskBudget NewDefaultDeskBudget(string code, string title)
    {
        return new DefaultDeskBudget(
            code,
            title,
            false,
            0.0,
            0.0,
            "",
            new Dictionary<string, string>(),
            "",
            0,
            "",
            0.0,
            DefaultDeskBudgetStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultDeskBudgetStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultDeskBudgetStatus next)
    {
        switch (Status)
        {
            case DefaultDeskBudgetStatus.Created:
                switch (next)
                {
                    case DefaultDeskBudgetStatus.Validated:
                    case DefaultDeskBudgetStatus.Published:
                        return true;
                }
                return false;
            case DefaultDeskBudgetStatus.Validated:
                switch (next)
                {
                    case DefaultDeskBudgetStatus.Retired:
                        return true;
                }
                return false;
            case DefaultDeskBudgetStatus.Published:
                switch (next)
                {
                    case DefaultDeskBudgetStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultDeskBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default desk budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"default desk budget"} {Code}";
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

    public double EffectiveLoad()
    {
        return Math.Round((double)Capacity + Rating, 2);
    }

    public DefaultDeskBudget Copy()
    {
        return new DefaultDeskBudget(
            Code,
            Title,
            Active,
            Rating,
            Hours,
            ManagerId,
            Attributes,
            Checksum,
            Capacity,
            Summary,
            Budget,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}

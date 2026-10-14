namespace Acme.Workforce.Compensation.MeritCycle.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a merit cycle tier is in its life cycle.</summary>
public enum MeritCycleTierStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One merit cycle tier, with what the planners need to act on it.</summary>
public sealed class MeritCycleTier(string code, string title, double fte, string grade, string description, bool pinned, string timezone, double weight, MeritCycleTierStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Fte { get; set; } = fte;
    public string Grade { get; set; } = grade;
    public string Description { get; set; } = description;
    public bool Pinned { get; set; } = pinned;
    public string Timezone { get; set; } = timezone;
    public double Weight { get; set; } = weight;
    public MeritCycleTierStatus Status { get; set; } = status;

    /// <summary>Starts a merit cycle tier in status created.</summary>
    public static MeritCycleTier NewMeritCycleTier(string code, string title)
    {
        return new MeritCycleTier(code, title, 0.0, "", "", false, "", 0.0, MeritCycleTierStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MeritCycleTierStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MeritCycleTierStatus next)
    {
        switch (Status)
        {
            case MeritCycleTierStatus.Created:
                switch (next)
                {
                    case MeritCycleTierStatus.Retired:
                        return true;
                }
                return false;
            case MeritCycleTierStatus.Validated:
                switch (next)
                {
                    case MeritCycleTierStatus.Published:
                        return true;
                }
                return false;
            case MeritCycleTierStatus.Published:
                switch (next)
                {
                    case MeritCycleTierStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(MeritCycleTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"merit cycle tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double AdjustedValue()
    {
        return Math.Round(Fte - Weight, 2);
    }

    public MeritCycleTier Copy()
    {
        return new MeritCycleTier(Code, Title, Fte, Grade, Description, Pinned, Timezone, Weight, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}

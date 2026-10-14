namespace Acme.Workforce.Payrollops.RetroPay.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a retro pay profile is in its life cycle.</summary>
public enum RetroPayProfileStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a retro pay profile and its current status.</summary>
public sealed class RetroPayProfile(string code, string title, int priority, Dictionary<string, string> attributes, double ceiling, double weight, RetroPayProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Priority { get; set; } = priority;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Ceiling { get; set; } = ceiling;
    public double Weight { get; set; } = weight;
    public RetroPayProfileStatus Status { get; set; } = status;

    /// <summary>Starts a retro pay profile in status created.</summary>
    public static RetroPayProfile NewRetroPayProfile(string code, string title)
    {
        return new RetroPayProfile(
            code,
            title,
            0,
            new Dictionary<string, string>(),
            0.0,
            0.0,
            RetroPayProfileStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RetroPayProfileStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RetroPayProfileStatus next)
    {
        switch (Status)
        {
            case RetroPayProfileStatus.Created:
                return next == RetroPayProfileStatus.Validated;
            case RetroPayProfileStatus.Validated:
                switch (next)
                {
                    case RetroPayProfileStatus.Published:
                    case RetroPayProfileStatus.Retired:
                        return true;
                }
                return false;
            case RetroPayProfileStatus.Published:
                switch (next)
                {
                    case RetroPayProfileStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RetroPayProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"retro pay profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
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

    public double AdjustedValue()
    {
        return Math.Round(Weight + Ceiling, 2);
    }

    public RetroPayProfile Copy()
    {
        return new RetroPayProfile(Code, Title, Priority, Attributes, Ceiling, Weight, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}

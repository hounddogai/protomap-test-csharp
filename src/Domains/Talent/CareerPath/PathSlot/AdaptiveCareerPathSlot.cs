namespace Acme.Workforce.Talent.CareerPath.PathSlot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive career path slot is in its life cycle.</summary>
public enum AdaptiveCareerPathSlotStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A adaptive career path slot as the career path team keeps it.</summary>
public sealed class AdaptiveCareerPathSlot(string code, string title, string name, double amount, int version, string jobCode, string locale, string skill, int attempts, string timezone, string description, AdaptiveCareerPathSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Name { get; set; } = name;
    public double Amount { get; set; } = amount;
    public int Version { get; set; } = version;
    public string JobCode { get; set; } = jobCode;
    public string Locale { get; set; } = locale;
    public string Skill { get; set; } = skill;
    public int Attempts { get; set; } = attempts;
    public string Timezone { get; set; } = timezone;
    public string Description { get; set; } = description;
    public AdaptiveCareerPathSlotStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive career path slot in status new.</summary>
    public static AdaptiveCareerPathSlot NewAdaptiveCareerPathSlot(string code, string title)
    {
        return new AdaptiveCareerPathSlot(
            code,
            title,
            "",
            0.0,
            0,
            "",
            "",
            "",
            0,
            "",
            "",
            AdaptiveCareerPathSlotStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveCareerPathSlotStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveCareerPathSlotStatus next)
    {
        switch (Status)
        {
            case AdaptiveCareerPathSlotStatus.New:
                switch (next)
                {
                    case AdaptiveCareerPathSlotStatus.Queued:
                    case AdaptiveCareerPathSlotStatus.Failed:
                        return true;
                }
                return false;
            case AdaptiveCareerPathSlotStatus.Queued:
                switch (next)
                {
                    case AdaptiveCareerPathSlotStatus.Running:
                    case AdaptiveCareerPathSlotStatus.Failed:
                        return true;
                }
                return false;
            case AdaptiveCareerPathSlotStatus.Running:
                switch (next)
                {
                    case AdaptiveCareerPathSlotStatus.Done:
                        return true;
                }
                return false;
            case AdaptiveCareerPathSlotStatus.Done:
                return next == AdaptiveCareerPathSlotStatus.Failed;
        }
        return false;
    }

    public void MoveTo(AdaptiveCareerPathSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive career path slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double WeightedTotal()
    {
        return Math.Round(Amount + (double)Attempts, 2);
    }

    public AdaptiveCareerPathSlot Copy()
    {
        return new AdaptiveCareerPathSlot(
            Code,
            Title,
            Name,
            Amount,
            Version,
            JobCode,
            Locale,
            Skill,
            Attempts,
            Timezone,
            Description,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}

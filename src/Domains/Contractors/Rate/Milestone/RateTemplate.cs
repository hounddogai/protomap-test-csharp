namespace Acme.Workforce.Contractors.Rate.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rate template is in its life cycle.</summary>
public enum RateTemplateStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One rate template, with what the planners need to act on it.</summary>
public sealed class RateTemplate(string code, string name, double weight, int attempts, double rate, string locale, int graceDays, string source, string title, string checksum, RateTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Weight { get; set; } = weight;
    public int Attempts { get; set; } = attempts;
    public double Rate { get; set; } = rate;
    public string Locale { get; set; } = locale;
    public int GraceDays { get; set; } = graceDays;
    public string Source { get; set; } = source;
    public string Title { get; set; } = title;
    public string Checksum { get; set; } = checksum;
    public RateTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a rate template in status new.</summary>
    public static RateTemplate NewRateTemplate(string code, string name)
    {
        return new RateTemplate(code, name, 0.0, 0, 0.0, "", 0, "", "", "", RateTemplateStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RateTemplateStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RateTemplateStatus next)
    {
        switch (Status)
        {
            case RateTemplateStatus.New:
                return next == RateTemplateStatus.Queued || next == RateTemplateStatus.Done;
            case RateTemplateStatus.Queued:
                switch (next)
                {
                    case RateTemplateStatus.Done:
                        return true;
                }
                return false;
            case RateTemplateStatus.Running:
                switch (next)
                {
                    case RateTemplateStatus.Done:
                    case RateTemplateStatus.Failed:
                        return true;
                }
                return false;
            case RateTemplateStatus.Done:
                switch (next)
                {
                    case RateTemplateStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RateTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rate template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"rate template"} {Code}";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)Attempts + Weight, 2);
    }

    public RateTemplate Copy()
    {
        return new RateTemplate(
            Code,
            Name,
            Weight,
            Attempts,
            Rate,
            Locale,
            GraceDays,
            Source,
            Title,
            Checksum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}

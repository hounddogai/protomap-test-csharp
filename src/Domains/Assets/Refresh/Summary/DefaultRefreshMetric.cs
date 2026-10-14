namespace Acme.Workforce.Assets.Refresh.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default refresh metric is in its life cycle.</summary>
public enum DefaultRefreshMetricStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A default refresh metric as the refresh team keeps it.</summary>
public sealed class DefaultRefreshMetric(string code, string name, List<string> tags, int retries, bool pinned, string checksum, bool active, double budget, double maximum, string skill, DefaultRefreshMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public List<string> Tags { get; set; } = tags;
    public int Retries { get; set; } = retries;
    public bool Pinned { get; set; } = pinned;
    public string Checksum { get; set; } = checksum;
    public bool Active { get; set; } = active;
    public double Budget { get; set; } = budget;
    public double Maximum { get; set; } = maximum;
    public string Skill { get; set; } = skill;
    public DefaultRefreshMetricStatus Status { get; set; } = status;

    /// <summary>Starts a default refresh metric in status new.</summary>
    public static DefaultRefreshMetric NewDefaultRefreshMetric(string code, string name)
    {
        return new DefaultRefreshMetric(
            code,
            name,
            new List<string>(),
            0,
            false,
            "",
            false,
            0.0,
            0.0,
            "",
            DefaultRefreshMetricStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultRefreshMetricStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultRefreshMetricStatus next)
    {
        switch (Status)
        {
            case DefaultRefreshMetricStatus.New:
                switch (next)
                {
                    case DefaultRefreshMetricStatus.Queued:
                    case DefaultRefreshMetricStatus.Running:
                        return true;
                }
                return false;
            case DefaultRefreshMetricStatus.Queued:
                switch (next)
                {
                    case DefaultRefreshMetricStatus.Running:
                    case DefaultRefreshMetricStatus.Done:
                        return true;
                }
                return false;
            case DefaultRefreshMetricStatus.Running:
                switch (next)
                {
                    case DefaultRefreshMetricStatus.Done:
                    case DefaultRefreshMetricStatus.Failed:
                        return true;
                }
                return false;
            case DefaultRefreshMetricStatus.Done:
                switch (next)
                {
                    case DefaultRefreshMetricStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultRefreshMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default refresh metric {Code} cannot move from {Status} to {next}");
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

    public double EffectiveLoad()
    {
        return Math.Round(Budget - (double)Retries, 2);
    }

    public DefaultRefreshMetric Copy()
    {
        return new DefaultRefreshMetric(
            Code,
            Name,
            Tags,
            Retries,
            Pinned,
            Checksum,
            Active,
            Budget,
            Maximum,
            Skill,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}

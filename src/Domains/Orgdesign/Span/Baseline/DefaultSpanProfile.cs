namespace Acme.Workforce.Orgdesign.Span.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default span profile is in its life cycle.</summary>
public enum DefaultSpanProfileStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A default span profile as the span team keeps it.</summary>
public sealed class DefaultSpanProfile(string code, string title, string label, string notes, string department, string phone, double weight, int sequence, bool visible, double threshold, List<string> tags, DefaultSpanProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Label { get; set; } = label;
    public string Notes { get; set; } = notes;
    public string Department { get; set; } = department;
    public string Phone { get; set; } = phone;
    public double Weight { get; set; } = weight;
    public int Sequence { get; set; } = sequence;
    public bool Visible { get; set; } = visible;
    public double Threshold { get; set; } = threshold;
    public List<string> Tags { get; set; } = tags;
    public DefaultSpanProfileStatus Status { get; set; } = status;

    /// <summary>Starts a default span profile in status proposed.</summary>
    public static DefaultSpanProfile NewDefaultSpanProfile(string code, string title)
    {
        return new DefaultSpanProfile(
            code,
            title,
            "",
            "",
            "",
            "",
            0.0,
            0,
            false,
            0.0,
            new List<string>(),
            DefaultSpanProfileStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultSpanProfileStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultSpanProfileStatus next)
    {
        switch (Status)
        {
            case DefaultSpanProfileStatus.Proposed:
                switch (next)
                {
                    case DefaultSpanProfileStatus.Withdrawn:
                        return true;
                }
                return false;
            case DefaultSpanProfileStatus.Approved:
                switch (next)
                {
                    case DefaultSpanProfileStatus.Rejected:
                        return true;
                }
                return false;
            case DefaultSpanProfileStatus.Rejected:
                switch (next)
                {
                    case DefaultSpanProfileStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultSpanProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default span profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Title}";
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

    public double AdjustedVolume()
    {
        return Math.Round((double)Sequence + Threshold, 2);
    }

    public DefaultSpanProfile Copy()
    {
        return new DefaultSpanProfile(
            Code,
            Title,
            Label,
            Notes,
            Department,
            Phone,
            Weight,
            Sequence,
            Visible,
            Threshold,
            Tags,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}

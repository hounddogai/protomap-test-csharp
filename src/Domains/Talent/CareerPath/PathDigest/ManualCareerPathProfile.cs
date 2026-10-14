namespace Acme.Workforce.Talent.CareerPath.PathDigest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual career path profile is in its life cycle.</summary>
public enum ManualCareerPathProfileStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A manual career path profile. Its code is unique within a tenant.</summary>
public sealed class ManualCareerPathProfile(string code, string name, string channel, Dictionary<string, string> attributes, string category, string unit, string phone, string description, double minimum, int retries, double weight, ManualCareerPathProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Channel { get; set; } = channel;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Category { get; set; } = category;
    public string Unit { get; set; } = unit;
    public string Phone { get; set; } = phone;
    public string Description { get; set; } = description;
    public double Minimum { get; set; } = minimum;
    public int Retries { get; set; } = retries;
    public double Weight { get; set; } = weight;
    public ManualCareerPathProfileStatus Status { get; set; } = status;

    /// <summary>Starts a manual career path profile in status idle.</summary>
    public static ManualCareerPathProfile NewManualCareerPathProfile(string code, string name)
    {
        return new ManualCareerPathProfile(
            code,
            name,
            "",
            new Dictionary<string, string>(),
            "",
            "",
            "",
            "",
            0.0,
            0,
            0.0,
            ManualCareerPathProfileStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualCareerPathProfileStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualCareerPathProfileStatus next)
    {
        switch (Status)
        {
            case ManualCareerPathProfileStatus.Idle:
                return next == ManualCareerPathProfileStatus.Draining || next == ManualCareerPathProfileStatus.Stopped;
            case ManualCareerPathProfileStatus.Warming:
                switch (next)
                {
                    case ManualCareerPathProfileStatus.Draining:
                        return true;
                }
                return false;
            case ManualCareerPathProfileStatus.Ready:
                return next == ManualCareerPathProfileStatus.Draining || next == ManualCareerPathProfileStatus.Stopped;
            case ManualCareerPathProfileStatus.Draining:
                return next == ManualCareerPathProfileStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(ManualCareerPathProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual career path profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
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

    public double WeightedVolume()
    {
        return Math.Round(Minimum * Weight, 2);
    }

    public ManualCareerPathProfile Copy()
    {
        return new ManualCareerPathProfile(
            Code,
            Name,
            Channel,
            Attributes,
            Category,
            Unit,
            Phone,
            Description,
            Minimum,
            Retries,
            Weight,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}

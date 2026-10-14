namespace Acme.Workforce.Orgdesign.Position.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global position variance is in its life cycle.</summary>
public enum GlobalPositionVarianceStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A global position variance as the position team keeps it.</summary>
public sealed class GlobalPositionVariance(string code, string title, Dictionary<string, string> attributes, string country, double rate, string salaryBand, double maximum, string name, int revision, int createdAt, int version, GlobalPositionVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Country { get; set; } = country;
    public double Rate { get; set; } = rate;
    public string SalaryBand { get; set; } = salaryBand;
    public double Maximum { get; set; } = maximum;
    public string Name { get; set; } = name;
    public int Revision { get; set; } = revision;
    public int CreatedAt { get; set; } = createdAt;
    public int Version { get; set; } = version;
    public GlobalPositionVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a global position variance in status requested.</summary>
    public static GlobalPositionVariance NewGlobalPositionVariance(string code, string title)
    {
        return new GlobalPositionVariance(
            code,
            title,
            new Dictionary<string, string>(),
            "",
            0.0,
            "",
            0.0,
            "",
            0,
            0,
            0,
            GlobalPositionVarianceStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalPositionVarianceStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalPositionVarianceStatus next)
    {
        switch (Status)
        {
            case GlobalPositionVarianceStatus.Requested:
                switch (next)
                {
                    case GlobalPositionVarianceStatus.Granted:
                        return true;
                }
                return false;
            case GlobalPositionVarianceStatus.Granted:
                switch (next)
                {
                    case GlobalPositionVarianceStatus.Revoked:
                    case GlobalPositionVarianceStatus.Expired:
                        return true;
                }
                return false;
            case GlobalPositionVarianceStatus.Revoked:
                return next == GlobalPositionVarianceStatus.Expired;
        }
        return false;
    }

    public void MoveTo(GlobalPositionVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global position variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{"global position variance"} {Code}";
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
        return Math.Round((double)CreatedAt - Rate, 2);
    }

    public GlobalPositionVariance Copy()
    {
        return new GlobalPositionVariance(
            Code,
            Title,
            Attributes,
            Country,
            Rate,
            SalaryBand,
            Maximum,
            Name,
            Revision,
            CreatedAt,
            Version,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}

namespace Acme.Workforce.Assets.AssetReturn.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a asset return variance is in its life cycle.</summary>
public enum AssetReturnVarianceStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a asset return variance and its current status.</summary>
public sealed class AssetReturnVariance(string code, string name, List<string> tags, double weight, string timezone, string channel, int sequence, double margin, AssetReturnVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public List<string> Tags { get; set; } = tags;
    public double Weight { get; set; } = weight;
    public string Timezone { get; set; } = timezone;
    public string Channel { get; set; } = channel;
    public int Sequence { get; set; } = sequence;
    public double Margin { get; set; } = margin;
    public AssetReturnVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a asset return variance in status proposed.</summary>
    public static AssetReturnVariance NewAssetReturnVariance(string code, string name)
    {
        return new AssetReturnVariance(
            code,
            name,
            new List<string>(),
            0.0,
            "",
            "",
            0,
            0.0,
            AssetReturnVarianceStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssetReturnVarianceStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssetReturnVarianceStatus next)
    {
        switch (Status)
        {
            case AssetReturnVarianceStatus.Proposed:
                switch (next)
                {
                    case AssetReturnVarianceStatus.Rejected:
                        return true;
                }
                return false;
            case AssetReturnVarianceStatus.Approved:
                switch (next)
                {
                    case AssetReturnVarianceStatus.Rejected:
                    case AssetReturnVarianceStatus.Withdrawn:
                        return true;
                }
                return false;
            case AssetReturnVarianceStatus.Rejected:
                return next == AssetReturnVarianceStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(AssetReturnVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"asset return variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
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

    public double AdjustedLoad()
    {
        return Math.Round(Margin - (double)Sequence, 2);
    }

    public AssetReturnVariance Copy()
    {
        return new AssetReturnVariance(Code, Name, Tags, Weight, Timezone, Channel, Sequence, Margin, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}

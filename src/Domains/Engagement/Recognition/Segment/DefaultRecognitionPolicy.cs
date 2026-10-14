namespace Acme.Workforce.Engagement.Recognition.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default recognition policy is in its life cycle.</summary>
public enum DefaultRecognitionPolicyStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a default recognition policy and its current status.</summary>
public sealed class DefaultRecognitionPolicy(string code, string name, int priority, string segment, double floorValue, DefaultRecognitionPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Priority { get; set; } = priority;
    public string Segment { get; set; } = segment;
    public double FloorValue { get; set; } = floorValue;
    public DefaultRecognitionPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a default recognition policy in status created.</summary>
    public static DefaultRecognitionPolicy NewDefaultRecognitionPolicy(string code, string name)
    {
        return new DefaultRecognitionPolicy(code, name, 0, "", 0.0, DefaultRecognitionPolicyStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultRecognitionPolicyStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultRecognitionPolicyStatus next)
    {
        switch (Status)
        {
            case DefaultRecognitionPolicyStatus.Created:
                switch (next)
                {
                    case DefaultRecognitionPolicyStatus.Validated:
                    case DefaultRecognitionPolicyStatus.Published:
                        return true;
                }
                return false;
            case DefaultRecognitionPolicyStatus.Validated:
                return next == DefaultRecognitionPolicyStatus.Published || next == DefaultRecognitionPolicyStatus.Retired;
            case DefaultRecognitionPolicyStatus.Published:
                return next == DefaultRecognitionPolicyStatus.Retired;
        }
        return false;
    }

    public void MoveTo(DefaultRecognitionPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default recognition policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"default recognition policy"} {Code}";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)Priority * FloorValue, 2);
    }

    public DefaultRecognitionPolicy Copy()
    {
        return new DefaultRecognitionPolicy(Code, Name, Priority, Segment, FloorValue, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}

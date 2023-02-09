﻿namespace Gbm.Challenge.Domain.Timestamp;

public class Epoch
{
    static readonly DateTime epochStart = new(1970, 1, 1, 0, 0, 0);

    public static DateTime FromUnix(long secondsSinceepoch)
    {
        return epochStart.AddSeconds(secondsSinceepoch);
    }

    public static long ToUnix(DateTime dateTime)
    {
        return (long)(dateTime - epochStart).TotalSeconds;
    }
}
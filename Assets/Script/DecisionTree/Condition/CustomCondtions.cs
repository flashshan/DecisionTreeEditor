using UnityEngine;


public class TimeCondition : BasicCondition
{
    [DtVariable("TimePhases", "Time Phase (0 - 24)")]
    public Vector2[] availableTimeRanges_ = new Vector2[0];

    public TimeCondition() : base() { }  // for editor only

    public override void Initialize()
    {
        base.Initialize();

        for (int i = 0; i < availableTimeRanges_.Length; ++i)
        {
            if (availableTimeRanges_[i].x < -0.001f || availableTimeRanges_[i].y < -0.001)
            {
                Debug.LogError("Time Condition's value should not below 0");
            }
            if (availableTimeRanges_[i].x > availableTimeRanges_[i].y)
            {
                Debug.LogError("Time Condition's end should larger than its begin");
            }
            if (availableTimeRanges_[i].x > 24.001f || availableTimeRanges_[i].y > 24.001f)
            {
                Debug.LogError("Time Condition's value should not above 24");
            }
        }
    }

    
    public override bool IsConditionMet()
    {
        if (!base.IsConditionMet())
            return false;

        float curTime = EnvironmentManager.GetInstance().timeOfDay_;
        for (int i = 0; i < availableTimeRanges_.Length; ++i)
        {
            if (curTime >= availableTimeRanges_[i].x && curTime < availableTimeRanges_[i].y)
            {
                return true;
            }
        }
        return false;
    }
}

public class TemperatureCondition : BasicCondition
{
    [DtVariable("TemperatureRange", "Temperature Range (0 - 40)")]
    public Vector2[] availableTempRanges_ = new Vector2[0];

    public TemperatureCondition() : base() { }  // for editor only

    public override void Initialize()
    {
        base.Initialize();

        for (int i = 0; i < availableTempRanges_.Length; ++i)
        {
            if (availableTempRanges_[i].x < -0.001f || availableTempRanges_[i].y < -0.001)
            {
                Debug.LogError("Temperature Condition's value should not below 0");
            }
            if (availableTempRanges_[i].x > availableTempRanges_[i].y)
            {
                Debug.LogError("Temperature Condition's end should larger than its begin");
            }
            if (availableTempRanges_[i].x > 40.001f || availableTempRanges_[i].y > 40.001f)
            {
                Debug.LogError("Temperature Condition's value should not above 40");
            }
        }
    }


    public override bool IsConditionMet()
    {
        if (!base.IsConditionMet())
            return false;

        float curTime = EnvironmentManager.GetInstance().temperature_;
        for (int i = 0; i < availableTempRanges_.Length; ++i)
        {
            if (curTime >= availableTempRanges_[i].x && curTime < availableTempRanges_[i].y)
            {
                return true;
            }
        }
        return false;
    }
}

public class UnderWaterCondition : BasicCondition
{
    [DtVariable("RequireUnderGround", "Require Under Water")]
    public bool requireUnderWater_;

    public UnderWaterCondition() : base() { }  // for editor only

    public override bool IsConditionMet()
    {
        if (!base.IsConditionMet())
            return false;

        return EnvironmentManager.GetInstance().isUnderWater_ == requireUnderWater_;
    }
}

public class PlayerRunCondition : BasicCondition
{
    [DtVariable("requirePlayerRun", "Require player run")]
    public bool requirePlayerRun_;

    public PlayerRunCondition() : base() { }  // for editor only

    public override bool IsConditionMet()
    {
        if (!base.IsConditionMet())
            return false;

        return EnvironmentManager.GetInstance().playerRun_ == requirePlayerRun_;
    }
}

public class PlayerShootCondition : BasicCondition
{
    [DtVariable("RequirePlayerShoot", "Require Player Shoot")]
    private bool requirePlayerShoot_;

    public PlayerShootCondition() : base() { }  // for editor only

    public override bool IsConditionMet()
    {
        if (!base.IsConditionMet())
            return false;

        return EnvironmentManager.GetInstance().playerShoot_ == requirePlayerShoot_;
    }
}
   

//public class PerformanceAboveHighCondition : BasicCondition
//{
//    [DtVariable("IsOverload", "Has Performance issue: ")]
//    public bool _performanceIsHigh;

//    public PerformanceAboveHighCondition() : base() { }  // for editor only

//    public override bool IsConditionMet()
//    {
//        if (!base.IsConditionMet())
//            return false;

//       
//    }
//}

public class TestCondition : BasicCondition
{
    [DtVariable("testBool", "Bool")]
    public bool testBool_;
    [DtVariable("testInt", "Int")]
    public int testInt_;
    [DtVariable("testFloat", "Float")]
    public float testFloat_;
    [DtVariable("testVector2", "Vector2")]
    public Vector2 testVector2_;
    [DtVariable("testVector3", "Vector3")]
    public Vector3 testVector3_;
    [DtVariable("testVector4", "Vector4")]
    public Vector4 testVector4_;
    [DtVariable("testColor", "Color")]
    public Color testColor_;

    [DtVariable("testBoolArray", "Bool Array")]
    public bool[] testBoolArray_ = new bool[0];
    [DtVariable("testIntArray", "Int Array")]
    public int[] testIntArray_ = new int[0];
    [DtVariable("testFloatArray", "Float Array")]
    public float[] testFloatArray_ = new float[0];
    [DtVariable("testVector2Array", "Vector2 Array")]
    public Vector2[] testVector2Array_ = new Vector2[0];
    [DtVariable("testVector3Array", "Vector3 Array")]
    public Vector3[] testVector3Array_ = new Vector3[0];
    [DtVariable("testVector4Array", "Vector4 Array")]
    public Vector4[] testVector4Array_ = new Vector4[0];
    [DtVariable("testColorArray", "Color Array")]
    public Color[] testColorArray_ = new Color[0];

    public TestCondition() : base() { }  // for editor only

    public override bool IsConditionMet()
    {
        if (!base.IsConditionMet())
            return false;

        return true;
    }

}

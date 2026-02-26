using System;

namespace Utils
{
	public static class WeaponMethods
	{
		public static int GetWeakestDefenseType(UnitModel target)
		{
			float r = target.defense.R;
			float w = target.defense.W;
			float b = target.defense.B;
			float p = target.defense.P;
			int num = 0;
			float num2 = float.MinValue;
			if (r > num2)
			{
				num2 = r;
				num = 0;
			}
			if (w > num2)
			{
				num2 = w;
				num = 1;
			}
			if (b > num2)
			{
				num2 = b;
				num = 2;
			}
			if (p > num2)
			{
				num = 3;
			}
			return num + 1;
		}
		public static bool HasImmuneDefense(UnitModel target)
		{
			return target.defense.R <= 0f || target.defense.W <= 0f || target.defense.B <= 0f || target.defense.P <= 0f;
		}
	}
}

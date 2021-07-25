using System.Collections.Generic;
using System.Reflection.Emit;
using Verse;
using RimWorld;
using HarmonyLib;

namespace NamesGaloreLatin
{
    class Patcher : GameComponent
    {
        private static float curSolidNameProbability = 0.5f;
        private static float curNicknameProbability = 0.15f;
        private static Harmony _harmony;
        private static Harmony harmony
        {
            get {
                if (_harmony==null)
                {
                    _harmony = new Harmony("rimworld.falconelaris.NamesGaloreLatin.patcher");
                }
                return _harmony;
            }
            set { _harmony = value; }
        }

        public Patcher() => ApplyPatches();

        public Patcher(Game g) => ApplyPatches();

        //public override void FinalizeInit()
        private void ApplyPatches()
        {
#if DEBUG
            Harmony.DEBUG = true;
#endif
            if (curSolidNameProbability != NamesGaloreLatinMod.settings.solidNameProbability)
            {
#if DEBUG
                Log.Message("NamesGaloreLatin: Patching Solid Name Probability");
#endif
                harmony.Patch(AccessTools.Method(typeof(PawnBioAndNameGenerator), nameof(PawnBioAndNameGenerator.GeneratePawnName)), null, null, new HarmonyMethod(typeof(Patcher), nameof(ReduceSolidNameChance)));
                curSolidNameProbability = NamesGaloreLatinMod.settings.solidNameProbability;
            }

            if (curNicknameProbability != NamesGaloreLatinMod.settings.nicknameProbability)
            {
#if DEBUG
                Log.Message("NamesGaloreLatin: Patching Nickname Probability");
#endif
                harmony.Patch(AccessTools.Method(typeof(PawnBioAndNameGenerator), "GeneratePawnName_Shuffled"), null, null, new HarmonyMethod(typeof(Patcher), nameof(AdjustNicknameChance)));
                curNicknameProbability = NamesGaloreLatinMod.settings.nicknameProbability;
            }

            harmony = null;
#if DEBUG
            Harmony.DEBUG = false;
#endif
        }

        private static IEnumerable<CodeInstruction> ReduceSolidNameChance(IEnumerable<CodeInstruction> instructions)
        {
            return FloatTranspileroo(instructions, curSolidNameProbability, NamesGaloreLatinMod.settings.solidNameProbability);
        }

        private static IEnumerable<CodeInstruction> AdjustNicknameChance(IEnumerable<CodeInstruction> instructions)
        {
            return FloatTranspileroo(instructions, curNicknameProbability, NamesGaloreLatinMod.settings.nicknameProbability);
        }

        private static IEnumerable<CodeInstruction> FloatTranspileroo(IEnumerable<CodeInstruction> instructions, float origVal, float newVal)
        {
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Ldc_R4 && (float)instruction.operand == origVal)
                    yield return new CodeInstruction(OpCodes.Ldc_R4, newVal);
                else
                    yield return instruction;
            }
        }
    }

}

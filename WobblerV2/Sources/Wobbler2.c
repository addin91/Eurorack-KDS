#include "Wobbler2.h"
#include <Math.h>
#include "../../EurorackShared/EURORACKSHARED.H"

#define __max(a,b) ((a)>(b)?(a):(b))
#define __min(a,b) ((a)<(b)?(a):(b))

#ifdef __cplusplus
extern "C"
{
#endif

	void Wobbler2_RandomSeed(struct Wobbler2_RandomGen *R, unsigned int seed)
	{
		R->RandomMemory = (long)seed;
	}

	void Wobbler2_SyncPulse(struct Wobbler2_LFO *LFO)
	{
		//int Delta = timesincesync;
		//if (Delta > 3000)
		{
		//	LFO->
		}
	}

	int Wobbler2_Rand(struct Wobbler2_RandomGen *R)
	{
		return (((R->RandomMemory = R->RandomMemory * 214013L + 2531011L) >> 16) & 0x7fff);
	}

	void Wobbler2_Init(struct Wobbler2_LFO *LFO)
	{
		LFO->Output = 0;
		LFO->OutputPhased = 0;
		LFO->Phase1 = 0;
		LFO->Gate[0] = 0;
		LFO->Gate[1] = 0;
		LFO->EnvelopeVal = 0;
		LFO->PhasedCountdown = 0;
		LFO->EnvelopeState = Wobbler2_IDLE;
		Wobbler2_InitPendulum(&LFO->Pendulum);
	}

	void Wobbler2_Trigger(struct Wobbler2_LFO *LFO, unsigned char N, struct Wobbler2_Params *Params)
	{
		if (N == 0)
		{
			LFO->Phase1 = 0;
			Wobbler2_StartTwang(LFO);
		}
	}

	void Wobbler2_LoadSettings(struct Wobbler2_Settings *settings, struct Wobbler2_Params *params)
	{

	}

	void Wobbler2_ValidateParams(struct Wobbler2_Params *params)
	{

	}

	unsigned long Wobbler2_LFORange(int32_t V, int32_t SR)
	{
		return  1 + ((V*SR * 64)>>8);
		//	return (unsigned long)(64 * pow((int32_t)((SR * 6) / 64.0), pow((int32_t)V, 0.54f)));
	}

	unsigned long Wobbler2_LFORange2(int32_t V)
	{
		return  1 + V*V *8;
		//	return (unsigned long)(64 * pow((int32_t)((SR * 6) / 64.0), pow((int32_t)V, 0.54f)));
	}



	void Wobbler2_InitPendulum(struct Wobbler2_Pendulum *P)
	{
		P->A = 0;
		P->B = 0;
		P->m1 = 1;
		P->m2 = 1;
		P->Theta1 = 3.1415 / 2.0f;
		P->Theta2 = 3.1415 / 2.0f;
		P->d2Theta1 = 0;
		P->d2Theta2 = 0;
		P->dTheta1 = 0;
		P->dTheta2 = 0;
		P->l1 = 0.2;
		P->l2 = 0.2;
		P->g = .981;
		P->mu = 1 + P->m1 / P->m2;
	}

	void Wobbler2_DoublePendulum(struct Wobbler2_Pendulum *P, float DT)
	{
		float st1 = sin(P->Theta1);
		float st2 = sin(P->Theta2);
		float _2sub1 =P->Theta2 - P->Theta1;
		float _1sub2 =P->Theta1 - P->Theta2;
		float c1sub2 = cos(_1sub2);
		float s1sub2 = sin(_1sub2);
		P->d2Theta1 = (P->g*(st2*c1sub2 - P->mu*st1) - (P->l2*P->dTheta2*P->dTheta2 + P->l1*P->dTheta1*P->dTheta1*c1sub2)*s1sub2) / (P->l1*(P->mu - c1sub2*c1sub2));
		P->d2Theta2 = (P->mu*P->g*(st1*c1sub2 - st2) + (P->mu*P->l1*P->dTheta1*P->dTheta1 + P->l2*P->dTheta2*P->dTheta2*c1sub2)*s1sub2) / (P->l2*(P->mu - c1sub2*c1sub2));
		P->dTheta1 *= 0.999;
		P->dTheta2 *= 0.999;
		P->dTheta1 += P->d2Theta1*DT;
		P->dTheta2 += P->d2Theta2*DT;
		P->Theta1 += P->dTheta1*DT;
		P->Theta2 += P->dTheta2*DT;
		P->A = P->Theta1 * 0xffff;
		P->B = P->Theta2 * 0xffff;
	}

	void Wobbler2_InitIntPendulum(struct Wobbler2_PendulumInt *P)
	{
		P->A = 0;
		P->B =0 ;
		P->m1 = 1<<16;
		P->m2 = 1<<16;
		P->Theta1 = 0x1000000;
		P->Theta2 = 0x1000000;
		P->d2Theta1 = 0;
		P->d2Theta2 = 0;
		P->dTheta1 = 0;
		P->dTheta2 = 0;
		P->l1 = (int)(0.2*(1<<16));
		P->l2 = (int)(0.2*(1<<16));
		P->g = (int)(.981*(1<<16));
		P->mu = (1 << 16) + ((P->m1 / P->m2) << 16);
	}
	
	int32_t mul16x16(int32_t a, int32_t b)
	{
		int64_t T = (int64_t)a * (int64_t)b;
		return (T/ ((1<<32) - 1));
	}

	void Wobbler2_DoublePendulumInt(struct Wobbler2_PendulumInt *P, float DT)
	{
		int32_t st1 = Sine(P->Theta1);
		int32_t st2 = Sine(P->Theta2);
		int32_t _2sub1 = P->Theta2 - P->Theta1;
		int32_t _1sub2 = P->Theta1 - P->Theta2;
		int32_t c1sub2 = Cosine(_1sub2);
		int32_t s1sub2 = Sine(_1sub2);

		int32_t th1Top = (mul16x16(P->g, (mul16x16(st2, c1sub2) - mul16x16(P->mu, st1))) - mul16x16((mul16x16(mul16x16(P->l2, P->dTheta2), P->dTheta2) + mul16x16(mul16x16(mul16x16(P->l1, P->dTheta1), P->dTheta1), c1sub2)), s1sub2));
		int32_t th1Bot  = (mul16x16(P->l1 , (P->mu - mul16x16(c1sub2 ,c1sub2))));
		P->d2Theta1 = th1Top / th1Bot;

		P->d2Theta2 = mul16x16(mul16x16(mul16x16(P->mu , P->g) , (mul16x16(st1 ,c1sub2 ) - st2)) +
				(
					mul16x16(mul16x16(mul16x16(P->mu , P->l1 ), P->dTheta1 ), P->dTheta1)
						+ 
					mul16x16(mul16x16(mul16x16(P->l2 , P->dTheta2), P->dTheta2 ),c1sub2)
				),s1sub2) 
			/ (mul16x16(P->l2,(P->mu - mul16x16(c1sub2,c1sub2))));
		P->dTheta1 = mul16x16(P->dTheta1, (int)(0.997*(1<<16)));
		P->dTheta2 = mul16x16(P->dTheta2, (int)(0.997*(1 << 16)));
		P->dTheta1 += mul16x16(P->d2Theta1,DT);
		P->dTheta2 += mul16x16(P->d2Theta2,DT);
		P->Theta1 += mul16x16(P->dTheta1,DT);
		P->Theta2 += mul16x16(P->dTheta2,DT);
		P->A = P->Theta1;
		P->B = P->Theta2;
	}


	void Wobbler2_StartTwang(struct Wobbler2_LFO *LFO)
	{
		//LFO->EnvelopeVal = 0;
		LFO->EnvelopeState = Wobbler2_ATTACK;
	}

	int Wobbler2_SampleHold(struct Wobbler2_LFO_SNH *sh, struct Wobbler2_LFO *lfo, uint32_t phase, uint16_t mod)
	{
		int newseg = (phase >> 29);
		SetSVF(&sh->filt, 0x10 + mod / 4, 0x150);

		if (newseg != sh->lastseg)
		{
			if (newseg == 0)
			{
				Wobbler2_RandomSeed(&sh->random, lfo->Phasing);
			}
			sh->lastseg = newseg;
			sh->lastval = (Wobbler2_Rand(&sh->random) << 14) - (1 << 28);
		}
		ProcessSVF(&sh->filt, sh->lastval >> 16);
		return sh->filt.lo;
	}

	int Wobbler2_Twang(struct Wobbler2_LFO *LFO, uint32_t phase)
	{
		return (Sine(phase) >> 16) * (LFO->EnvelopeVal >> 8);
	}

	int Wobbler2_Get(struct Wobbler2_LFO *LFO, struct Wobbler2_Params *Params)
	{
		if (LFO->Gate[0] > 0)
		{
			LFO->Gate[0]--;
		}
		if (LFO->Gate[1] > 0) {
			LFO->Gate[1]--;
		}

		if (LFO->EnvelopeState != Wobbler2_IDLE)
		{
			uint32_t A = 0;
			uint32_t R = LFORange(128, 2000) >> 12;
			if (LFO->Mod < 128)
			{
				R = 1 + (LFORange(LFO->Mod, 2000) >> 12);
			}
			else
			{
				A = 1 + (LFORange(LFO->Mod - (128<<8), 2000) >> 12);
			}
			if (LFO->EnvelopeState == Wobbler2_ATTACK)
			{
				if (A == 0)
				{
					LFO->EnvelopeState = Wobbler2_RELEASE;
					LFO->EnvelopeVal = 1 << 24;
				}
				else
				{
					LFO->EnvelopeVal += ((1 << 24) - 1) / A;
					if (LFO->EnvelopeVal >= 1 << 24)
					{
						LFO->EnvelopeVal = 1 << 24;
						LFO->EnvelopeState = Wobbler2_RELEASE;
					}
				}
			}
			else
			{
				LFO->EnvelopeVal -= ((1 << 24) - 1) / R;
				if (LFO->EnvelopeVal <= 0)
				{
					LFO->EnvelopeState = Wobbler2_IDLE;
					LFO->EnvelopeVal = 0;
				}
			}
		}


		uint32_t DP = LFORange2(LFO->Speed);;
		LFO->Phase1 += DP;

		uint32_t DP2 = LFO->Phasing * 0x100000;
		//DP2 <<= 24;
		LFO->Phase2 = LFO->Phase1 + DP2;

		for (int i = 0; i < 9; i++)
		{
			LFO->Led[0][i] = 0;
			LFO->Led[1][i] = 0;
		}

		if (LFO->Phase1 < LFO->OldPhase1)
		{
			LFO->Gate[1] = Wobbler2_GATECOUNTDOWN;
			if (LFO->PhasedCountdown > 0)
			{
				LFO->Gate[0] = Wobbler2_GATECOUNTDOWN;
			}
			LFO->PhasedCountdown = LFO->Phasing << 24;
		}

		uint32_t last = LFO->PhasedCountdown;
		LFO->PhasedCountdown -= __min(DP, LFO->PhasedCountdown);
		if (LFO->PhasedCountdown == 0 && last != 0)
		{
			LFO->Gate[0] = Wobbler2_GATECOUNTDOWN;
		}

		LFO->OldPhase1 = LFO->Phase1;
		LFO->OldPhase2 = LFO->Phase2;

		int32_t O[4];
		int32_t P[4];
		Wobbler2_DoublePendulum(&LFO->Pendulum, 0.05f);
		O[0] = BasicShapes(LFO->Phase1, LFO->Mod>>8);
		O[1] = Wobbler2_Twang(LFO, LFO->Phase1);
		O[2] = Wobbler2_SampleHold(&LFO->SNH[0], LFO, LFO->Phase1, LFO->Mod);

		O[2] = LFO->Pendulum.A;

		P[0] = BasicShapes(LFO->Phase2, LFO->Mod>>8);
		P[1] = Wobbler2_Twang(LFO, LFO->Phase2);
		P[2] = Wobbler2_SampleHold(&LFO->SNH[1], LFO, LFO->Phase2, LFO->Mod);
		P[2] = LFO->Pendulum.B;


		struct SteppedResult_t sr;
		GetSteppedResult(LFO->Shape, 2, &sr);

		LFO->Output = GetInterpolatedResultInt(O, &sr) /(0xffff*4);
		 LFO->OutputPhased = GetInterpolatedResultInt(P, &sr) /(0xffff*4);


		LFO->Output += 2048;// + (2540 - 2048);
		LFO->OutputPhased += 2048;// +(2540 - 2048);

		if (LFO->Output > 4095) LFO->Output = 4095; else if (LFO->Output < 0) LFO->Output = 0;
		if (LFO->OutputPhased > 4095) LFO->OutputPhased = 4095; else if (LFO->OutputPhased < 0) LFO->OutputPhased = 0;
		//if (LFO->Output > 1) LFO->Output = 1; else if (LFO->Output < -1) LFO->Output = -1;

		int32_t LedIdxB = (LFO->Output * 8);
		int iLedIdxB = LedIdxB >> 12;
		int IdxB = ((LedIdxB - (iLedIdxB << 12))) >> 4;

		LFO->Led[0][(iLedIdxB + 9) % 9] = 255 - IdxB;
		LFO->Led[0][(iLedIdxB + 10) % 9] = IdxB;

		int32_t LedIdxA = (LFO->OutputPhased * 8);
		int iLedIdxA = LedIdxA >> 12;
		int IdxA = ((LedIdxA - (iLedIdxA << 12))) >> 4;

		LFO->Led[1][(iLedIdxA + 9) % 9] = 255 - IdxA;
		LFO->Led[1][(iLedIdxA + 10) % 9] = IdxA;
		for(int i = 0;i<3;i++){LFO->ModeLed[i] = 0;};
		LFO->ModeLed[sr.index] = sr.fractional;
		LFO->ModeLed[sr.index+1] = 255-sr.fractional;

		return LFO->Output;
	}

	int32_t GetInterpolatedResultInt(int32_t *table, SteppedResult_t *inp)
	{
		unsigned char F = inp->fractional;
		unsigned char IF =  ~inp->fractional;

		return ((table[inp->index]/256) * IF)  + ((table[inp->index + 1]/256) * F);
	}

	void GetSteppedResult(uint16_t param, uint8_t steps, SteppedResult_t *out)
	{
		//max(floor(x + 0.25), (x + 0.25 - floor(x + 0.25)) * 2 + floor(x + 0.25) - 1)

		uint32_t X = (param * steps)/256;
		X += 64;

		int FloorX = X &0xffffff00;
		int Aside = FloorX;
		int Bside = (X - (FloorX)) * 2 + (FloorX)-256;
		int M = (Aside > Bside) ? Aside : Bside;

		out->index = M >> 8;
		out->fractional = M & 0xff;

	}


#ifdef __cplusplus
}
#endif
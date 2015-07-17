//==============================================================
//  The source from the Tony Lee edited and updated, without
//  the author's permission is prohibited acts on the source
//  code for commercial purposes or other reproduced and explained.
//==============================================================
//  Copyright(C) 2014 - 2015 Tony Lee
//  All rights reserved
//	Input Item: RandomPlus
//	PC Name:    PC0525
//	Name Space: TerumoMIS.CoreLibrary
//	File Name:  RandomPlus
//	User name:  C1400008
//	Location Time: 2015/7/10 8:31:12
//  Tony Lee [mailto:liting5828424@gmail.com]
//==============================================================
//  Update History :
//  CLRVersion : 4.0.30319.18408
//==============================================================

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

namespace TerumoMIS.CoreLibrary
{
    /// <summary>
    /// 随机数
    /// </summary>
    public unsafe sealed class RandomPlus
    {
        /// <summary>
        /// 公用种子
        /// </summary>
        private uint* _seeds;
        /// <summary>
        /// 安全种子
        /// </summary>
        private uint* _secureSeeds;
        /// <summary>
        /// 32位种子位置
        /// </summary>
        private int _current;
        /// <summary>
        /// 64位种子位置
        /// </summary>
        private int _current64;
        /// <summary>
        /// 64位种子位置访问锁
        /// </summary>
        private int _currentLock;
        /// <summary>
        /// 随机位缓存
        /// </summary>
        private uint _bits;
        /// <summary>
        /// 随机位缓存数量
        /// </summary>
        private int _bitCount;
        /// <summary>
        /// 字节缓存访问锁
        /// </summary>
        private int _byteLock;
        /// <summary>
        /// 字节缓存
        /// </summary>
        private ulong _bytes;
        /// <summary>
        /// 字节缓存数量
        /// </summary>
        private int _byteCount;
        /// <summary>
        /// 双字节缓存访问锁
        /// </summary>
        private int _ushortLock;
        /// <summary>
        /// 双字节缓存
        /// </summary>
        private ulong _ushorts;
        /// <summary>
        /// 双字节缓存数量
        /// </summary>
        private int _ushortCount;
        /// <summary>
        /// 随机数
        /// </summary>
        private RandomPlus()
        {
            _secureSeeds = unmanaged.Get(64 * sizeof(uint) + 5 * 11 * sizeof(uint), false).UInt;
            _seeds = _secureSeeds + 64;
            _current64 = 5 * 11 - 2;
            var tick = (ulong)PubPlus.StartTime.Ticks ^ (ulong)Environment.TickCount ^ ((ulong)PubPlus.Identity32 << 8) ^ ((ulong)date.NowTimerInterval << 24);
            var isSeedArray = 0;
            var seedField = typeof(Random).GetField("SeedArray", BindingFlags.Instance | BindingFlags.NonPublic);
            if (seedField != null)
            {
                var seedArray = seedField.GetValue(new Random()) as int[];
                if (seedArray != null && seedArray.Length == 5 * 11 + 1)
                {
                    tick *= 0xb163dUL;
                    fixed (int* seedFixed = seedArray)
                    {
                        for (uint* write = _seeds, end = _seeds + 5 * 11, read = (uint*)seedFixed; write != end; tick >>= 1)
                        {
                            *write++ = *++read ^ (((uint)tick & 1U) << 31);
                        }
                    }
                    isSeedArray = 1;
                }
            }
            if (isSeedArray == 0)
            {
                LogPlus.Default.Add("系统随机数种子获取失败");
                for (uint* start = _seeds, end = start + 5 * 11; start != end; ++start)
                {
                    *start = (uint)tick ^ (uint)(tick >> 32);
                    tick *= 0xb163dUL;
                    tick += tick >> 32;
                }
            }
            for (var start = (ulong*)_secureSeeds; start != _seeds; *start++ = NextULong())
            {
            }
            _bits = (uint)Next();
            _bitCount = 32;
        }
        /// <summary>
        /// 获取随机种子位置
        /// </summary>
        /// <returns></returns>
        private int NextIndex()
        {
            var index = Interlocked.Increment(ref _current);
            if (index >= 5 * 11)
            {
                int cacheIndex = index;
                do
                {
                    index -= 5 * 11;
                }
                while (index >= 5 * 11);
                Interlocked.CompareExchange(ref _current, index, cacheIndex);
            }
            return index;
        }
        /// <summary>
        /// 获取下一个随机数
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Next()
        {
            int index = NextIndex();
            uint* seed = _seeds + index;
            if (index < (5 * 11 - 3 * 7)) return (int)(*seed -= *(seed + 3 * 7));
            return (int)(*seed ^= *(seed - (5 * 11 - 3 * 7)));
        }
        /// <summary>
        /// 获取下一个随机数
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float NextFloat()
        {
            int index = NextIndex();
            uint* seed = _seeds + index;
            if (index < (5 * 11 - 3 * 7)) *seed -= *(seed + 3 * 7);
            else *seed ^= *(seed - (5 * 11 - 3 * 7));
            return *(float*)seed;
        }
        /// <summary>
        /// 获取下一个随机数
        /// </summary>
        /// <param name="mod">求余取模数</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Next(int mod)
        {
            if (mod <= 1) return 0;
            int value = Next() % mod;
            return value >= 0 ? value : (value + mod);
        }
        /// <summary>
        /// 获取下一个随机位
        /// </summary>
        /// <returns></returns>
        public uint NextBit()
        {
            var count = Interlocked.Decrement(ref _bitCount);
            while (count < 0)
            {
                Thread.Sleep(0);
                count = Interlocked.Decrement(ref _bitCount);
            }
            if (count == 0)
            {
                uint value = _bits & 1;
                _bits = (uint)Next();
                _bitCount = 32;
                return value;
            }
            return _bits & (1U << count);
        }
        /// <summary>
        /// 获取下一个随机字节
        /// </summary>
        /// <returns></returns>
        public byte NextByte()
        {
        START:
            interlocked.NoCheckCompareSetSleep0(ref _byteLock);
            if (_byteCount == 0)
            {
                _byteCount = -1;
                _byteLock = 0;
                byte value = (byte)(_bytes = NextULong());
                _bytes >>= 8;
                interlocked.NoCheckCompareSetSleep0(ref _byteLock);
                _byteCount = 7;
                _byteLock = 0;
                return value;
            }
            else if (_byteCount > 0)
            {
                var value = (byte)_bytes;
                --_byteCount;
                _bytes >>= 8;
                _byteLock = 0;
                return value;
            }
            else
            {
                _byteLock = 0;
                Thread.Sleep(0);
                goto START;
            }
        }
        /// <summary>
        /// 获取下一个随机双字节
        /// </summary>
        /// <returns></returns>
        public ushort NextUShort()
        {
        START:
            interlocked.NoCheckCompareSetSleep0(ref _ushortLock);
            if (_ushortCount == 0)
            {
                _ushortLock = 0;
                ushort value = (ushort)(_ushorts = NextULong());
                _ushorts >>= 16;
                interlocked.NoCheckCompareSetSleep0(ref _ushortLock);
                _ushortCount = 3;
                _ushortLock = 0;
                return value;
            }
            if (_ushortCount > 0)
            {
                ushort value = (ushort)_ushorts;
                --_ushortCount;
                _ushorts >>= 16;
                _ushortLock = 0;
                return value;
            }
            else
            {
                _ushortLock = 0;
                Thread.Sleep(0);
                goto START;
            }
        }
        /// <summary>
        /// 获取随机种子位置
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int NextIndex64()
        {
            interlocked.NoCheckCompareSetSleep0(ref _currentLock);
            var index = _current64;
            if ((_current64 -= 2) < 0) _current64 = (5 * 11 - 4) - _current64;
            _currentLock = 0;
            return index;
        }
        /// <summary>
        /// 获取下一个随机数
        /// </summary>
        public ulong NextULong()
        {
            int index = NextIndex64();
            uint* seed = _seeds + index;
            if (index < (5 * 11 - 3 * 7 - 1)) return *(ulong*)seed -= *(ulong*)(seed + 3 * 7);
            if (index == (5 * 11 - 3 * 7 - 1)) return *(ulong*)seed -= *(ulong*)_seeds;
            return *(ulong*)seed ^= *(ulong*)(seed - (5 * 11 - 3 * 7));
        }
        /// <summary>
        /// 获取下一个随机数
        /// </summary>
        public double NextDouble()
        {
            var index = NextIndex64();
            var seed = _seeds + index;
            if (index < (5 * 11 - 3 * 7 - 1)) *(ulong*)seed -= *(ulong*)(seed + 3 * 7);
            else if (index == (5 * 11 - 3 * 7 - 1)) *(ulong*)seed -= *(ulong*)_seeds;
            else *(ulong*)seed ^= *(ulong*)(seed - (5 * 11 - 3 * 7));
            return *(double*)seed;
        }
        /// <summary>
        /// 获取下一个随机数
        /// </summary>
        public int SecureNext()
        {
            int seed = Next(), leftIndex = seed & 63, rightIndex = (seed >> 6) & 63;
            if (leftIndex == rightIndex) return (int)((_secureSeeds[leftIndex] ^= (uint)seed) - (uint)seed);
            if ((seed & (1 << ((seed >> 12) & 31))) == 0) return (int)((_secureSeeds[leftIndex] -= _secureSeeds[rightIndex]) ^ (uint)seed);
            return (int)((_secureSeeds[leftIndex] ^= _secureSeeds[rightIndex]) - (uint)seed);
        }
        /// <summary>
        /// 获取下一个随机数
        /// </summary>
        public ulong SecureNextULong()
        {
            ulong seed = NextULong();
            int leftIndex = (int)(uint)seed & 63, rightIndex = (int)((uint)seed >> 6) & 63;
            if (leftIndex == 63) leftIndex = 62;
            if (rightIndex == 63) rightIndex = 62;
            if (leftIndex == rightIndex) return (*(ulong*)(_secureSeeds + leftIndex) ^= seed) - seed;
            if (((uint)seed & (1U << ((int)((uint)seed >> 12) & 31))) == 0) return (*(ulong*)(_secureSeeds + leftIndex) -= *(ulong*)(_secureSeeds + rightIndex)) ^ seed;
            return (*(ulong*)(_secureSeeds + leftIndex) ^= *(ulong*)(_secureSeeds + rightIndex)) - seed;
        }
        /// <summary>
        /// 获取下一个非0随机数
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong SecureNextULongNotZero()
        {
            ulong value = SecureNextULong();
            while (value == 0) value = SecureNextULong();
            return value;
        }
        /// <summary>
        /// 默认随机数
        /// </summary>
        public static RandomPlus Default = new RandomPlus();
        /// <summary>
        /// 随机Hash值
        /// </summary>
        internal static readonly int Hash = Default.Next();
    }
}

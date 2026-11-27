using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace Volcano.Interface
{
    public static class ApplicationPriorityOptimizer
    {
        // --- P/Invoke Signatures ---
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetProcessPriorityBoost(IntPtr hProcess, bool bDisablePriorityBoost);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetProcessInformation(
            IntPtr hProcess,
            int ProcessInformationClass,
            ref PROCESS_POWER_THROTTLING_STATE ProcessInformation,
            uint ProcessInformationSize);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentProcess();

        // --- Win32 Constants & Structs ---
        private const int ProcessPowerThrottling = 4;

        private const uint PROCESS_POWER_THROTTLING_CURRENT_VERSION = 1;
        private const uint PROCESS_POWER_THROTTLING_EXECUTION_SPEED = 0x1;

        [StructLayout(LayoutKind.Sequential)]
        private struct PROCESS_POWER_THROTTLING_STATE
        {
            public uint Version;
            public uint ControlMask;
            public uint StateMask;
        }

        /// <summary>
        /// Forces the application into maximum performance mode.
        /// Elevates priority, prevents OS priority decay, and disables Win11 EcoQoS/E-Core throttling.
        /// </summary>
        public static void EnableMaximumPerformanceMode()
        {
            IntPtr currentProcessHandle = GetCurrentProcess();

            // 1. Elevate Process & Thread Priority Classes
            try
            {
                using (Process currentProcess = Process.GetCurrentProcess())
                {
                    // Tries RealTime if Admin, automatically falls back to High if restricted
                    currentProcess.PriorityClass = ProcessPriorityClass.High;
                }
                Thread.CurrentThread.Priority = ThreadPriority.Highest;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Priority] Could not adjust PriorityClass: {ex.Message}");
            }

            // 2. Lock Priority Boosts (Disable Dynamic Decay)
            if (!SetProcessPriorityBoost(currentProcessHandle, true))
            {
                int error = Marshal.GetLastWin32Error();
                Debug.WriteLine($"[Priority Boost] Failed to lock priority boost. Win32 Error: {error}");
            }

            // 3. Opt Out of Windows 11 Power Throttling / EcoQoS (Keep on P-Cores)
            var throttlingState = new PROCESS_POWER_THROTTLING_STATE
            {
                Version = PROCESS_POWER_THROTTLING_CURRENT_VERSION,
                ControlMask = PROCESS_POWER_THROTTLING_EXECUTION_SPEED,
                StateMask = 0 // 0 explicitly turns execution throttling OFF
            };

            bool success = SetProcessInformation(
                currentProcessHandle,
                ProcessPowerThrottling,
                ref throttlingState,
                (uint)Marshal.SizeOf(throttlingState)
            );

            if (!success)
            {
                Debug.WriteLine($"[Power Throttling] Could not disable EcoQoS. Win32 Error: {Marshal.GetLastWin32Error()}");
            }
        }
    }
}
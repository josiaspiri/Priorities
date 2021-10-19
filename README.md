# Priorities

An attempt unlikely to increase foreground process performance while very likely increasing system instability on Windows.

## How?

On run sets all accessible processes to idle priority and uses a system hook to set foreground process priority to realtime.
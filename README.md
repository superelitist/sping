# sping
Simultaneous ping, for when you need lots of ping.

```
>_ .\sping.exe
sping 1.0.0
Copyright (C) 2022 sping

ERROR(S):
  A required value not bound to option name is missing.

  -c, --count           (Default: 1) The total number of echo requests to make.

  -p, --payload         (Default: 32) Byte payload of packet.

  -t, --timeout         (Default: 1000) Maximum time (in milliseconds) to wait for the reply.

  -T, --timeToLive      (Default: 128) Maximum number of hops before packet should be discarded.

  -f, --dontFragment    (Default: false) Whether the packet is allowed to be fragmented.

  -v, --verbose         Set output to verbose messages.

  --help                Display this help screen.

  --version             Display version information.

  address (pos. 0)      Required. The hostname or IP address.
  ```

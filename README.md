# NetTextureBenchmark
.net texture channel merge (SpecularGlossiness) test 
Test are realized using two differents approach.
One using a simple Get/Set Pixel schem.
The other using direct access to image memory in unsafe mode and task parallel lib support
result is straighforward


|           Method |         Mean |       Error |      StdDev |
|----------------- |-------------:|------------:|------------:|
| MergeGetSetPixel | 694,101.7 us | 5,032.83 us | 4,461.47 us |
|    MergeLockbits |     957.3 us |    12.07 us |    11.29 us |

with

 Mean   : Arithmetic mean of all measurements
 Error  : Half of 99.9% confidence interval
 StdDev : Standard deviation of all measurements
 1 us   : 1 Microsecond (0.000001 sec)

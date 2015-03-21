I have created deobfuscator for .Net for myself. It works nice on small and medium applications, but crashes on big app and I have no time to find what is wrong. Code is very simple and anyone can improve it.


Deobfuscator was based on simple obfuscator NCloak. Main idea is that deobfuscated application should work.

There some unique features:

1. Smart class, field and method renaming. Detecting forms, buttons, edits, labels and so on.

2. Tracing with Console.Write('method\_name'). This feature can show what methods have been called, very useful when deobfuscated app refuses to run.

3. Strong name token replacing. Many protection systems encrypt strings and resources with strong name. Deobfuscator reads original key and replaces system calls for getting that key to new method, which returns byte array with original key.

4. Simple IL code cleaning (against CryptoObfuscator).
# Kbtter4 Polypropylene
kb10uy  
Livetを基盤としてCoreTweetを使うTwitterクライアント

## 指針
* Kbtter3のようなコードによるコントロールの生成に依存しない。コードビハインドを徹底的に減らす。
* Kbtter2よりも大幅にMVVM化する。
* Kbtter1,2,3のようにタブ化しない。
* Krile STARRYEYESっぽいUIを目指す。
* アイコンをもっときれいにする。
* BehaviorとDataTemplateでどうにか出来そうなところはどうにかする。

##機能
* 一般的なTwitterクライアントの機能
* マルチアカウント
* IronPython,Luaによるプラグイン機能
* 独自クエリによるユーザー定義タイムライン
 - Krile、Shrimpのそれとは別物
 - Kbtter3からの引き継ぎ

##構成
* Kbtter4
 - メインの実行ファイル
* Kbtter4.Cache
 - キャッシュ機構専用ライブラリ
* Kbtter4.Tenko
 - コマンドライン専用ライブラリ
* Kbtter3.Cache
 - クエリ用ライブラリ

## 参考にしました
Kbtter3はいろいろなTwitterクライアントを参考にします。

|内容                                  |クライアント名    |作者TwitterのSN  |
|:-------------------------------------|:-----------------|:----------------|
|テクニック                            |Kbtter1,2,3       |kb10uy           |
|余計なものを表示しないというようなアレ|Soarer            |java_shit        |
|大本なUIデザイン                      |Krile             |karno            |
|イケてるUIという志向                  |Javaビーム工房    |orekyuu          |
|新機能のアイデア                      |ななせったーの構想|nanase_coder     |

##Kbtterのソースについて
MIT Licenseとします。
LICENSE.txtを参照してください。

##Special Thanks
TwitterのSNの昇順です。  
実際に開発に協力してくれた人以外にも、個人的にSpecial Thanksな人が入る可能性が大いにあります。

* [@aayh](https://twitter.com/aayh)
* [@alalwww](https://twitter.com/alalwww)
* [@azyobuzin](https://twitter.com/azyobuzin)
 - 外部サービス画像展開に[img.azyobuzi.net](http://img.azyobuzi.net)を利用しています。
   ありがとうございます。超便利です。
* [@func_hs](https://twitter.com/func_hs)
* [@g2simm](https://twitter.com/g2simm)
* [@hiyaudon](https://twitter.com/hiyaudon)
* [@java_shit](https://twitter.com/java_shit)
* [@karno](https://twitter.com/karno)
* [@kb10uy](https://twitter.com/kb10uy)
* [@laco0416](https://twitter.com/laco0416)
* [@mumei_himazin](https://twitter.com/mumei_himazin)
* [@nanase_coder](https://twitter.com/nanase_coder)
* [@noko_k](https://twitter.com/noko_k)
* [@orekyuu](https://twitter.com/orekyuu)
* [@otack](https://twitter.com/otack)
* [@paralleltree](https://twitter.com/paralleltree)
* [@sukirer](https://twitter.com/sukirer)
* [@syuilo](https://twitter.com/syuilo)
* [@taiki_hacker](https://twitter.com/taiki_hacker)
* [@toshi_a](https://twitter.com/toshi_a)
* [@tsuteto](https://twitter.com/tsuteto)
* [@ulicknormanowen](https://twitter.com/ulicknormanowen)

##ライセンス
* KbtterのソースのライセンスはLICENSE.txtを参照してください。
* Kbtterは、Apache License 2.0のライブラリを使用しています。
* Kbtterは、多くのライブラリを参照使用しています。  
  参照しているライブラリのライセンスは以下を参照してください。
  各ライブラリの使用については、それぞれの条文に従ってください。
 - WPF Color Dialogを使用しています。WPF Color Dialogはフリーウェアです。
   http://www.kanazawa-net.ne.jp/~pmansato/wpf/wpf_custom_ColorPicker.htm
 - WPF Brush Editorを使用しています。WPF Brush Editorはフリーウェアです。
   http://www.kanazawa-net.ne.jp/~pmansato/wpf/wpf_custom_BrushEditor.htm
 - 以下はMIT Licenseのライブラリです。
  + CoreTweet
  + JSON.NET
  + NLua
  + Irony
  + Dapper
 - 以下はzlib/pngライセンスのライブラリです。
  + Livet
 - 以下はApache License 2.0のライブラリです。
  + Reactive Extensions
  + IronPython
 - 以下はPublic Domainのライブラリです。
  + SQLite
  + System.Data.SQLite
 - Entity Frameworkは、マイクロソフト ソフトウェア ライセンス条項下のライブラリです。  
   詳しくは[マイクロソフト ソフトウェア ライセンス条項](http://www.microsoft.com/web/webpi/eula/net_library_eula_jpn.htm)を参照してください。

###使用ライブラリのライセンス

####Apache License 2.0
------------------
> Apache License
> Version 2.0, January 2004
> http://www.apache.org/licenses/ 
> 
> TERMS AND CONDITIONS FOR USE, REPRODUCTION, AND DISTRIBUTION 
> 
> 1. Definitions.
> 
> "License" shall mean the terms and conditions for use, reproduction, and distribution as defined by Sections 1 through 9 of this document. 
> 
> "Licensor" shall mean the copyright owner or entity authorized by the copyright owner that is granting the License. 
> 
> "Legal Entity" shall mean the union of the acting entity and all other entities that control, are controlled by, or are under common control with that entity. For the purposes of this definition, "control" means (i) the power, direct or indirect, to cause the direction or management of such entity, whether by contract or otherwise, or (ii) ownership of fifty percent (50%) or more of the outstanding shares, or (iii) beneficial ownership of such entity. 
> 
> "You" (or "Your") shall mean an individual or Legal Entity exercising permissions granted by this License. 
> 
> "Source" form shall mean the preferred form for making modifications, including but not limited to software source code, documentation source, and configuration files. 
> 
> "Object" form shall mean any form resulting from mechanical transformation or translation of a Source form, including but not limited to compiled object code, generated documentation, and conversions to other media types. 
> 
> "Work" shall mean the work of authorship, whether in Source or Object form, made available under the License, as indicated by a copyright notice that is included in or attached to the work (an example is provided in the Appendix below). 
> 
> "Derivative Works" shall mean any work, whether in Source or Object form, that is based on (or derived from) the Work and for which the editorial revisions, annotations, elaborations, or other modifications represent, as a whole, an original work of authorship. For the purposes of this License, Derivative Works shall not include works that remain separable from, or merely link (or bind by name) to the interfaces of, the Work and Derivative Works thereof. 
> 
> "Contribution" shall mean any work of authorship, including the original version of the Work and any modifications or additions to that Work or Derivative Works thereof, that is intentionally submitted to Licensor for inclusion in the Work by the copyright owner or by an individual or Legal Entity authorized to submit on behalf of the copyright owner. For the purposes of this definition, "submitted" means any form of electronic, verbal, or written communication sent to the Licensor or its representatives, including but not limited to communication on electronic mailing lists, source code control systems, and issue tracking systems that are managed by, or on behalf of, the Licensor for the purpose of discussing and improving the Work, but excluding communication that is conspicuously marked or otherwise designated in writing by the copyright owner as "Not a Contribution." 
> 
> "Contributor" shall mean Licensor and any individual or Legal Entity on behalf of whom a Contribution has been received by Licensor and subsequently incorporated within the Work. 
> 
> 2. Grant of Copyright License. Subject to the terms and conditions of this License, each Contributor hereby grants to You a perpetual, worldwide, non-exclusive, no-charge, royalty-free, irrevocable copyright license to reproduce, prepare Derivative Works of, publicly display, publicly perform, sublicense, and distribute the Work and such Derivative Works in Source or Object form. 
> 
> 3. Grant of Patent License. Subject to the terms and conditions of this License, each Contributor hereby grants to You a perpetual, worldwide, non-exclusive, no-charge, royalty-free, irrevocable (except as stated in this section) patent license to make, have made, use, offer to sell, sell, import, and otherwise transfer the Work, where such license applies only to those patent claims licensable by such Contributor that are necessarily infringed by their Contribution(s) alone or by combination of their Contribution(s) with the Work to which such Contribution(s) was submitted. If You institute patent litigation against any entity (including a cross-claim or counterclaim in a lawsuit) alleging that the Work or a Contribution incorporated within the Work constitutes direct or contributory patent infringement, then any patent licenses granted to You under this License for that Work shall terminate as of the date such litigation is filed. 
> 
> 4. Redistribution. You may reproduce and distribute copies of the Work or Derivative Works thereof in any medium, with or without modifications, and in Source or Object form, provided that You meet the following conditions: 
> 
> You must give any other recipients of the Work or Derivative Works a copy of this License; and 
> 
> 
> You must cause any modified files to carry prominent notices stating that You changed the files; and 
> 
> 
> You must retain, in the Source form of any Derivative Works that You distribute, all copyright, patent, trademark, and attribution notices from the Source form of the Work, excluding those notices that do not pertain to any part of the Derivative Works; and 
> 
> 
> If the Work includes a "NOTICE" text file as part of its distribution, then any Derivative Works that You distribute must include a readable copy of the attribution notices contained within such NOTICE file, excluding those notices that do not pertain to any part of the Derivative Works, in at least one of the following places: within a NOTICE text file distributed as part of the Derivative Works; within the Source form or documentation, if provided along with the Derivative Works; or, within a display generated by the Derivative Works, if and wherever such third-party notices normally appear. The contents of the NOTICE file are for informational purposes only and do not modify the License. You may add Your own attribution notices within Derivative Works that You distribute, alongside or as an addendum to the NOTICE text from the Work, provided that such additional attribution notices cannot be construed as modifying the License. 
> You may add Your own copyright statement to Your modifications and may provide additional or different license terms and conditions for use, reproduction, or distribution of Your modifications, or for any such Derivative Works as a whole, provided Your use, reproduction, and distribution of the Work otherwise complies with the conditions stated in this License. 
> 
> 5. Submission of Contributions. Unless You explicitly state otherwise, any Contribution intentionally submitted for inclusion in the Work by You to the Licensor shall be under the terms and conditions of this License, without any additional terms or conditions. Notwithstanding the above, nothing herein shall supersede or modify the terms of any separate license agreement you may have executed with Licensor regarding such Contributions. 
> 
> 6. Trademarks. This License does not grant permission to use the trade names, trademarks, service marks, or product names of the Licensor, except as required for reasonable and customary use in describing the origin of the Work and reproducing the content of the NOTICE file. 
> 
> 7. Disclaimer of Warranty. Unless required by applicable law or agreed to in writing, Licensor provides the Work (and each Contributor provides its Contributions) on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied, including, without limitation, any warranties or conditions of TITLE, NON-INFRINGEMENT, MERCHANTABILITY, or FITNESS FOR A PARTICULAR PURPOSE. You are solely responsible for determining the appropriateness of using or redistributing the Work and assume any risks associated with Your exercise of permissions under this License. 
> 
> 8. Limitation of Liability. In no event and under no legal theory, whether in tort (including negligence), contract, or otherwise, unless required by applicable law (such as deliberate and grossly negligent acts) or agreed to in writing, shall any Contributor be liable to You for damages, including any direct, indirect, special, incidental, or consequential damages of any character arising as a result of this License or out of the use or inability to use the Work (including but not limited to damages for loss of goodwill, work stoppage, computer failure or malfunction, or any and all other commercial damages or losses), even if such Contributor has been advised of the possibility of such damages. 
> 
> 9. Accepting Warranty or Additional Liability. While redistributing the Work or Derivative Works thereof, You may choose to offer, and charge a fee for, acceptance of support, warranty, indemnity, or other liability obligations and/or rights consistent with this License. However, in accepting such obligations, You may act only on Your own behalf and on Your sole responsibility, not on behalf of any other Contributor, and only if You agree to indemnify, defend, and hold each Contributor harmless for any liability incurred by, or claims asserted against, such Contributor by reason of your accepting any such warranty or additional liability. 

#### MIT License(s)
--------------------

> The MIT License (MIT)

> CoreTweet - A .NET Twitter Library supporting Twitter API 1.1
> Copyright (c) 2014 lambdalice

> Permission is hereby granted, free of charge, to any person obtaining a copy
> of this software and associated documentation files (the "Software"), to deal
> in the Software without restriction, including without limitation the rights
> to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
> copies of the Software, and to permit persons to whom the Software is
> furnished to do so, subject to the following conditions:
> 
> The above copyright notice and this permission notice shall be included in
> all copies or substantial portions of the Software.
> 
> THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
> IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
> FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
> AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
> LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
> OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
> THE SOFTWARE.


---------------------

> JSON.NET
> 
> The MIT License (MIT)
> 
> Copyright (c) 2007 James Newton-King
> 
> Permission is hereby granted, free of charge, to any person obtaining a copy of
> this software and associated documentation files (the "Software"), to deal in
> the Software without restriction, including without limitation the rights to
> use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
> the Software, and to permit persons to whom the Software is furnished to do so,
> subject to the following conditions:
> 
> The above copyright notice and this permission notice shall be included in all
> copies or substantial portions of the Software.
> 
> THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
> IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
> FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
> COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
> IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
> CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

---------

> ----------------
> NLua License
> --------------------
> 
> NLua is licensed under the terms of the MIT license reproduced below.
> This mean that NLua is free software and can be used for both academic and
> commercial purposes at absolutely no cost.
> 
> ===============================================================================
> 
> Copyright (C) 2013 - Vinicius Jarina (viniciusjarina@gmail.com)
> Copyright (C) 2012 Megax <http://megax.yeahunter.hu/>
> Copyright (C) 2003-2005 Fabio Mascarenhas de Queiroz.
> 
> 
> Permission is hereby granted, free of charge, to any person obtaining a copy
> of this software and associated documentation files (the "Software"), to deal
> in the Software without restriction, including without limitation the rights
> to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
> copies of the Software, and to permit persons to whom the Software is
> furnished to do so, subject to the following conditions:
> 
> The above copyright notice and this permission notice shall be included in
> all copies or substantial portions of the Software.
> 
> THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
> IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
> FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.  IN NO EVENT SHALL THE
> AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
> LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
> OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
> THE SOFTWARE.
> 
> ===============================================================================
> 
> Kopi Lua License
> ----------------
> 
> Kopi Lua is licensed under the terms of the MIT license. Both the MIT
> license and the original Lua copyright notice are reproduced below.
> 
> Please see http://www.ppl-pilot.com/KopiLua for details.
> 
> ===============================================================================
> 
> Lua License
> -----------
> 
> Lua is licensed under the terms of the MIT license reproduced below.
> This means that Lua is free software and can be used for both academic
> and commercial purposes at absolutely no cost.
> 
> For details and rationale, see http://www.lua.org/license.html .
> 
> ===============================================================================
> 
> Copyright (C) 1994-2008 Lua.org, PUC-Rio.
> 
> Permission is hereby granted, free of charge, to any person obtaining a copy
> of this software and associated documentation files (the "Software"), to deal
> in the Software without restriction, including without limitation the rights
> to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
> copies of the Software, and to permit persons to whom the Software is
> furnished to do so, subject to the following conditions:
> 
> The above copyright notice and this permission notice shall be included in
> all copies or substantial portions of the Software.
> 
> THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
> IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
> FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.  IN NO EVENT SHALL THE
> AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
> LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
> OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
> THE SOFTWARE.
> 
> ===============================================================================
(C) 2014 kb10uy

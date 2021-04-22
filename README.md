# About

This is a vulnerable ASP .net core application that implements all
vulnerabilities listed in [OWASP's Top 10](https://www.owasp.org/index.php/Category:OWASP_Top_Ten_Project)
as small web programs. For every web program, we provide an exploit
int the [exploits subdirectory](./exploits)
that documents how the vulnerability can be actually exploited.

# OWASP top 10 (2017)

At the moment, we have implemented the following vulnerabilities.

### A1 -Injection
- [x] SQL Injection
- [x] XPATH Injection
### A2 -Broken Authentication
- [x] Credential Stuffing
### A3 -Sensitive Data Exposure
- [x] Leaking Credit Card Information
### A4 -XML External Entities (XXE)
- [x] Accessing local resource
### A5 -Broken Access Control
- [x] Elevate access privileges
### A6 -Security Misconfiguration
- [x] Show SQL Exception in response
### A7 -Cross-Site Scripting (XSS)
- [x] Reflected XSS
### A8 -Insecure Deserialization
- [x] Insecure XML deserialization
### A9 -Using Components with Known Vulnerabilities
- [x] Using component vulnerable to XSS
### A10 -Insufficient Logging&Monitoring
- [x] Insufficient logging after data breach

# Preliminaries

The following instructions are based on the assumption that you are running
Linux/MacOS or, in case you are using Microsoft Windows, that you have 
[Cygwin](https://www.cygwin.com/) installed. We will port all the scripts to powershell so that they can be run natively soon.

We also assume that you have installed `csharp2cpg`, `cpg2sp` and `sl`.

# Installation

You can execute the following commands in order to run the vulnerable ASP .net
core application.

```bash
dotnet build vulnerable_asp_net_core.sln
dotnet run --project vulnerable_asp_net_core
```

After running the application, you should see the following output:

```bash
...
Now listening on: https://localhost:5001
Now listening on: http://localhost:5000
Application started. Press Ctrl+C to shut down.
Application is shutting down...
```

In order to run all exploits you can execute the `run_all.sh` script in the
`exploits/` directory with:

```bash
./run_all.sh
```

The command above should produce the following output:

```bash
execute ./sqlinjection.sh ... OK
execute ./xss.sh ... OK
execute ./vulnerable_component.sh ... OK
execute ./broken_authentication.sh ... OK
execute ./insecure_deserialization.sh ... OK
execute ./security_misconfiguration.sh ... OK
execute ./xxe.sh ... OK
execute ./common.sh ... OK
execute ./xpathinjection.sh ... OK
execute ./broken_access_control.sh ... OK
execute ./insufficient-logging.sh ... OK
execute ./sensitive_data_exposure.sh ... OK
```

You can consider the `./run_all.sh` script as a sanity check that you can
execute to verify that your setup works correctly.

# Howto find the vulnerabilities

## CPG Generation

After successfully setting up `csharp2cpg`, you can obtain the CPG
(in this example named `hsl.bin.zip`) of our vulnerable ASP .net core application by executing the
command below.

```bash
./csharp2cpg.sh -i vulnerable_asp_net_core/vulnerable_asp_net_core.csproj -o hsl.bin.zip
```

The CPG that corresponds `vulnerable_asp_net_core.csproj` is made available [here](https://drive.google.com/file/d/1FWsSorNcIQUtdI3SwfsZS9_4ATzTJC9X/view?usp=sharing).

## SP Generation


After successfully setting up `cpg2sp`, you can obtain the SP (in this example named `lm.sp`) of
our vulnerable ASP.net core application by executing the command below. The
`base.policy` file is available [here](https://drive.google.com/open?id=1d_90dzHDx3swA1-x4lpUOcHqjiJca6j2).

You can put the `base.policy` file in your policy directory (e.g., `$HOME/.shiftleft/policy/static/com/hsl`).

```bash
./cpg2sp.sh --cpg hsl.bin.zip -o lm.sp --platform csharp --overlay -p $HOME/.shiftleft/policy/static/com/hsl/base.policy
```

## Loading the SP in ocular 

For displaying the vulnerabilities in ocular you can execute the following
commands.


```bash
./ocular.sh

 ██████╗  ██████╗██╗   ██╗██╗      █████╗ ██████╗
██╔═══██╗██╔════╝██║   ██║██║     ██╔══██╗██╔══██╗
██║   ██║██║     ██║   ██║██║     ███████║██████╔╝
██║   ██║██║     ██║   ██║██║     ██╔══██║██╔══██╗
╚██████╔╝╚██████╗╚██████╔╝███████╗██║  ██║██║  ██║
 ╚═════╝  ╚═════╝ ╚═════╝ ╚══════╝╚═╝  ╚═╝╚═╝  ╚═╝
//...
ocular> loadCpgWithOverlays("hsl.bin.zip","lm.sp")
2019-02-21 16:25:53.408 [main] INFO Unzipping completed in 17ms.
SLF4J: Failed to load class "org.slf4j.impl.StaticLoggerBinder".
SLF4J: Defaulting to no-operation (NOP) logger implementation
SLF4J: See http://www.slf4j.org/codes.html#StaticLoggerBinder for further details.
2019-02-21 16:25:53.723 [main] INFO CPG construction finished in 313ms.
2019-02-21 16:25:53.746 [main] INFO Unzipping completed in 11ms.
res0: io.shiftleft.queryprimitives.steps.starters.Cpg = io.shiftleft.queryprimitives.steps.starters.Cpg@5d1e09bc

ocular> cpg.finding.p
res1: List[String] = List(
//...
  """------------------------------------
Title: Sensitive data contained in HTTP request/response via `creditCard` in `SL.SensitiveDataExposure`
Score: 2
Categories: [a6-sensitive-data-exposure]
Flow ids: [21646]
Description: Sensitive data included in HTTP request/response. This could result in sensitive data exposure. Many web applications and APIs do not properly protect sensitive data, such as financial and healthcare. Attackers may steal or modify such weakly protected data to conduct credit card fraud, identity theft, or other crimes.


## Countermeasures
//...
```

## Obtain analysis results on the dashboard

As a prerequisite for uploading the CPG to the dashboard, you need to setup the
[shiftleft cli](https://docs.shiftleft.io/shiftleft/getting-started/using-sl-the-shiftleft-cli).

You can run the analysis in the dashboard by executing the command below.

``` bash
./sl analyze --wait --policy com.hslNet/base --cpgupload --csharp --app HSLCS --force hsl.bin.zip

```
After launching the command above, you should be able to see output similar to the one in the excerpt below. At the bottom of the excerpt you will find a URL that you can paste in the address bar of your browser. After doing so, you will be directed to the dashboard that contains the findings. You can find a screenshot that illustrates what you should see in your dashboard below.

``` bash
Using Org ID found in local configuration file
Using Upload Token found in local configuration file
Uploading...

 84.39 KB / 84.39 KB [=================================================================] 100.00% 253.40 KB/s 0s

Saved config to shiftleft.json
... Done. Submitted for analysis
Waiting for analysis to finish. Press ctrl+c to cancel.
Progress: 15%
Progress: 15%
Progress: 16%
Progress: 30%
Progress: 31%
Progress: 90%
Progress: 91%
Progress: 100%
Done. Load the following URL in your browser:
https://www.stg.shiftleft.io/apps/HSLCS?organizationId=9494677f-8bdf-460f-afff-7ac09275e2a9
```

![Screenshot](https://github.com/ShiftLeftSecurity/testdata/blob/master/csharp/vulnerable_asp_net_core/img/hsl.png "Dashboard Results")

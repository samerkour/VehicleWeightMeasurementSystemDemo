# Rahdari (سامانه جامع راهداری) TTO WebService — Postman Curl Commands

Extracted from: «راهنمای فراخوانی وبسرویس ارسال اطلاعات به سامانه جامع (نسخه چهارم)»

- Protocol: SOAP 1.1 (XML)
- Endpoint (from `RmtoSync/appsettings.json`): `http://10.30.197.140:8080/`
- Account codes (must match this account or server returns error 128 `Invalid CompanyCode` / 105 `عدم صحت کد شرکت`):
  - `cOMPANYCODE` = **207**
  - `dEVICECODE` = **16953126**
  - `sYSTEMCODE` = **202**
  - `reserved7` (کد پلیس) = **607057**
- Headers for every request: `Content-Type: text/xml; charset=utf-8` and a `SOAPAction` header
- Namespaces:
  - soapenv = `http://schemas.xmlsoap.org/soap/envelope/`
  - tem = `http://tempuri.org/`
  - ttow = `http://schemas.datacontract.org/2004/07/TTOWS.Library.Models`
  - arr = `http://schemas.microsoft.com/2003/10/Serialization/Arrays`
- Date constraint: `PassDateTime` / `ReceiveDateTime` must be recent, otherwise server returns error 113 `Pass or Receive DateTime Expired`. Use today's date.

---

## 0. health — بررسی دسترسی به وبسرویس (قبل از دریافت نام کاربری)

Checks network + service access. No authentication needed; call it before Login. Returns `Result=200` when OK.

SOAPAction: `health`

```bash
curl --location "http://10.30.197.140:8080/" \
--header "Content-Type: text/xml; charset=utf-8" \
--header "SOAPAction: health" \
--data-raw '<?xml version="1.0" encoding="utf-8"?>
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:tem="http://tempuri.org/">
   <soapenv:Header/>
   <soapenv:Body>
      <tem:health/>
   </soapenv:Body>
</soapenv:Envelope>'
```

Verified live response (2026-08-08):
```xml
<tns:healthResult>
   <s5:ErrorCode>0</s5:ErrorCode>
   <s5:IsSuccessful>true</s5:IsSuccessful>
   <s5:Result>200</s5:Result>
</tns:healthResult>
```

---

## 1. Login — get operational token

SOAPAction: `Login`

```bash
curl --location "http://10.30.197.140:8080/" \
--header "Content-Type: text/xml; charset=utf-8" \
--header "SOAPAction: Login" \
--data-raw '<?xml version="1.0" encoding="utf-8"?>
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:tem="http://tempuri.org/">
   <soapenv:Header/>
   <soapenv:Body>
      <tem:Login>
         <tem:UserName>tabatozin</tem:UserName>
         <tem:Password>tab@#Tozin#123</tem:Password>
      </tem:Login>
   </soapenv:Body>
</soapenv:Envelope>'
```

Response: token is returned inside `<Result>...</Result>` — use it as `Token` in addTTOInfo / addTTOInfoBatch2.
Error: `1000` = نام کاربری یا کلمه عبور نادرست.

---

## 2. addTTOInfo — نمونه اول (پلاک استاندارد، بدون تخلف)

Traffic: car with plate «۰۹ گ ۹۹», speed 81 km/h, no violation, formatted plate.

SOAPAction: `addTTOInfo`

```bash
curl --location "http://10.30.197.140:8080/" \
--header "Content-Type: text/xml; charset=utf-8" \
--header "SOAPAction: addTTOInfo" \
--data-raw '<?xml version="1.0" encoding="utf-8"?>
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:tem="http://tempuri.org/" xmlns:ttow="http://schemas.datacontract.org/2004/07/TTOWS.Library.Models" xmlns:arr="http://schemas.microsoft.com/2003/10/Serialization/Arrays">
<soapenv:Body>
<tem:addTTOInfo>
<tem:Token>__TOKEN_FROM_LOGIN__</tem:Token>
<tem:TTOInfo>
<ttow:Crimes></ttow:Crimes>
<ttow:aLLOWED>1</ttow:aLLOWED>
<ttow:cOLORIMAGE>__BASE64_COLOR_IMAGE_GE_15KB__</ttow:cOLORIMAGE>
<ttow:cOMPANYCODE>207</ttow:cOMPANYCODE>
<ttow:dEVICECODE>16953126</ttow:dEVICECODE>
<ttow:hasImage>true</ttow:hasImage>
<ttow:lINENUMBER>1</ttow:lINENUMBER>
<ttow:pASSDATETIME>2026-08-08T00:02:10</ttow:pASSDATETIME>
<ttow:pLATEIMAGE>__BASE64_PLATE_1_50KB__</ttow:pLATEIMAGE>
<ttow:rECEIVEDATETIME>2026-08-08T00:04:13</ttow:rECEIVEDATETIME>
<ttow:rFIDNUMBER>2953930گ</ttow:rFIDNUMBER>
<ttow:ReferenceNo>32176190403</ttow:ReferenceNo>
<ttow:reserved7>607057</ttow:reserved7>
<ttow:sYSTEMCODE>202</ttow:sYSTEMCODE>
<ttow:speedType>1</ttow:speedType>
<ttow:vEHICLECLASS>1</ttow:vEHICLECLASS>
<ttow:vEHICLELEN>0</ttow:vEHICLELEN>
<ttow:vEHICLEPLATE>120234516</ttow:vEHICLEPLATE>
<ttow:vEHICLESPEED>81</ttow:vEHICLESPEED>
<ttow:wRONGDIRECTION>0</ttow:wRONGDIRECTION>
<ttow:LPF>1</ttow:LPF>
<ttow:SLPF>1</ttow:SLPF>
</tem:TTOInfo>
</tem:addTTOInfo>
</soapenv:Body>
</soapenv:Envelope>'
```

---

## 3. addTTOInfo — نمونه دوم (پلاک استاندارد + تخلف + سرعت متوسط)

Violation traffic: پلاک «۲۰۵ ۵ د ۲۵» → `651013550`, speed 90 km/h, avg speed 129 km/h, violation code 2310.

SOAPAction: `addTTOInfo`

```bash
curl --location "http://10.30.197.140:8080/" \
--header "Content-Type: text/xml; charset=utf-8" \
--header "SOAPAction: addTTOInfo" \
--data-raw '<?xml version="1.0" encoding="utf-8"?>
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:tem="http://tempuri.org/" xmlns:ttow="http://schemas.datacontract.org/2004/07/TTOWS.Library.Models" xmlns:arr="http://schemas.microsoft.com/2003/10/Serialization/Arrays">
   <soapenv:Header/>
   <soapenv:Body>
      <tem:addTTOInfo>
         <tem:Token>TOKEN_FROM_LOGIN</tem:Token>
         <tem:TTOInfo>
            <ttow:Crimes>
               <arr:Crime>
                  <arr:crime>2310</arr:crime>
               </arr:Crime>
            </ttow:Crimes>
            <ttow:aLLOWED>0</ttow:aLLOWED>
            <ttow:cOLORIMAGE>BASE64_COLOR_IMAGE</ttow:cOLORIMAGE>
            <ttow:cOMPANYCODE>207</ttow:cOMPANYCODE>
            <ttow:dEVICECODE>16953126</ttow:dEVICECODE>
            <ttow:hasImage>true</ttow:hasImage>
            <ttow:lINENUMBER>1</ttow:lINENUMBER>
            <ttow:pASSDATETIME>2026-08-08T00:00:03</ttow:pASSDATETIME>
            <ttow:pLATEIMAGE>BASE64_PLATE_IMAGE</ttow:pLATEIMAGE>
            <ttow:rECEIVEDATETIME>2026-08-08T00:00:05</ttow:rECEIVEDATETIME>
            <ttow:ReferenceNo>32176126256</ttow:ReferenceNo>
            <ttow:reserved10>129</ttow:reserved10>
            <ttow:reserved7>607057</ttow:reserved7>
            <ttow:sYSTEMCODE>202</ttow:sYSTEMCODE>
            <ttow:speedType>3</ttow:speedType>
            <ttow:vEHICLECLASS>1</ttow:vEHICLECLASS>
            <ttow:vEHICLELEN>0</ttow:vEHICLELEN>
            <ttow:vEHICLEPLATE>651013550</ttow:vEHICLEPLATE>
            <ttow:vEHICLESPEED>90</ttow:vEHICLESPEED>
            <ttow:wRONGDIRECTION>0</ttow:wRONGDIRECTION>
            <ttow:LPF>1</ttow:LPF>
            <ttow:SLPF>4</ttow:SLPF>
         </tem:TTOInfo>
      </tem:addTTOInfo>
   </soapenv:Body>
</soapenv:Envelope>'
```

---

## 4. addTTOInfo — نمونه سوم (پلاک بدون قالب مشخص — منطقه آزاد)

Plate not in standard format: `rFIDNUMBER = ARVAND9273733`, `vehiclePlate = 0`, speed 101 km/h, avg 127 km/h.

SOAPAction: `addTTOInfo`

```bash
curl --location "http://10.30.197.140:8080/" \
--header "Content-Type: text/xml; charset=utf-8" \
--header "SOAPAction: addTTOInfo" \
--data-raw '<?xml version="1.0" encoding="utf-8"?>
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:tem="http://tempuri.org/" xmlns:ttow="http://schemas.datacontract.org/2004/07/TTOWS.Library.Models" xmlns:arr="http://schemas.microsoft.com/2003/10/Serialization/Arrays">
   <soapenv:Header/>
   <soapenv:Body>
      <tem:addTTOInfo>
         <tem:Token>TOKEN_FROM_LOGIN</tem:Token>
         <tem:TTOInfo>
            <ttow:Crimes></ttow:Crimes>
            <ttow:aLLOWED>1</ttow:aLLOWED>
            <ttow:cOLORIMAGE>BASE64_COLOR_IMAGE</ttow:cOLORIMAGE>
            <ttow:cOMPANYCODE>207</ttow:cOMPANYCODE>
            <ttow:dEVICODE>18363109</ttow:dEVICODE>
            <ttow:eXCOLORIMAGE2></ttow:eXCOLORIMAGE2>
            <ttow:hasImage>true</ttow:hasImage>
            <ttow:lINENUMBER>1</ttow:lINENUMBER>
            <ttow:pASSDATETIME>2026-08-08T06:01:55</ttow:pASSDATETIME>
            <ttow:pLATEIMAGE>BASE64_PLATE_IMAGE</ttow:pLATEIMAGE>
            <ttow:rECEIVEDATETIME>2026-08-08T06:03:42</ttow:rECEIVEDATETIME>
            <ttow:rFIDNUMBER>ARVAND9273733</ttow:rFIDNUMBER>
            <ttow:ReferenceNo>1708345156341931206</ttow:ReferenceNo>
            <ttow:reserved10>127</ttow:reserved10>
            <ttow:reserved7>607057</ttow:reserved7>
            <ttow:sYSTEMCODE>202</ttow:sYSTEMCODE>
            <ttow:speedType>3</ttow:speedType>
            <ttow:vEHICLECLASS>1</ttow:vEHICLECLASS>
            <ttow:vEHICLEPLATE>0</ttow:vEHICLEPLATE>
            <ttow:vEHICLESPEED>101</ttow:vEHICLESPEED>
            <ttow:wRONGDIRECTION>0</ttow:wRONGDIRECTION>
            <ttow:LPF>1</ttow:LPF>
            <ttow:SLPF>4</ttow:SLPF>
         </tem:TTOInfo>
      </tem:addTTOInfo>
   </soapenv:Body>
</soapenv:Envelope>'
```

---

## 5. addTTOInfo — نمونه چهارم (خودرو سنگین / WIM، پلاک قرقیزستان)

Heavy vehicle via WIM: پلاک `FT538TF` (LPF=20 Kyrgyzstan, SLPF=0), total weight 15386 kg, 4 axles, CarClass13=10.

SOAPAction: `addTTOInfo`

```bash
curl --location "http://10.30.197.140:8080/" \
--header "Content-Type: text/xml; charset=utf-8" \
--header "SOAPAction: addTTOInfo" \
--data-raw '<?xml version="1.0" encoding="utf-8"?>
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:tem="http://tempuri.org/" xmlns:ttow="http://schemas.datacontract.org/2004/07/TTOWS.Library.Models" xmlns:arr="http://schemas.microsoft.com/2003/10/Serialization/Arrays">
   <soapenv:Header/>
   <soapenv:Body>
      <tem:addTTOInfo>
         <tem:Token>TOKEN_FROM_LOGIN</tem:Token>
         <tem:TTOInfo>
            <ttow:Crimes></ttow:Crimes>
            <ttow:aLLOWED>1</ttow:aLLOWED>
            <ttow:aXLESEQUIVALENT_W1>0</ttow:aXLESEQUIVALENT_W1>
            <ttow:aXLESEQUIVALENT_W2>0</ttow:aXLESEQUIVALENT_W2>
            <ttow:aXLESEQUIVALENT_W3>0</ttow:aXLESEQUIVALENT_W3>
            <ttow:aXLESEQUIVALENT_W4>0</ttow:aXLESEQUIVALENT_W4>
            <ttow:aXLESEQUIVALENT_W5>0</ttow:aXLESEQUIVALENT_W5>
            <ttow:aXLESEQUIVALENT_W6>0</ttow:aXLESEQUIVALENT_W6>
            <ttow:aXLESEQUIVALENT_W7>0</ttow:aXLESEQUIVALENT_W7>
            <ttow:aXLESEQUIVALENT_W8>0</ttow:aXLESEQUIVALENT_W8>
            <ttow:aXLESEQUIVALENT_W_MORETHAN8>0</ttow:aXLESEQUIVALENT_W_MORETHAN8>
            <ttow:aXLESWEIGHT1>4486</ttow:aXLESWEIGHT1>
            <ttow:aXLESWEIGHT2>4325</ttow:aXLESWEIGHT2>
            <ttow:aXLESWEIGHT3>3446</ttow:aXLESWEIGHT3>
            <ttow:aXLESWEIGHT4>3129</ttow:aXLESWEIGHT4>
            <ttow:aXLESWEIGHT5>0</ttow:aXLESWEIGHT5>
            <ttow:aXLESWEIGHT6>0</ttow:aXLESWEIGHT6>
            <ttow:aXLESWEIGHT7>0</ttow:aXLESWEIGHT7>
            <ttow:aXLESWEIGHT8>0</ttow:aXLESWEIGHT8>
            <ttow:aXLESWEIGHT9>0</ttow:aXLESWEIGHT9>
            <ttow:cARCLASS13>10</ttow:cARCLASS13>
            <ttow:cARCLASS15>10</ttow:cARCLASS15>
            <ttow:cOLORIMAGE>BASE64_COLOR_IMAGE</ttow:cOLORIMAGE>
            <ttow:cOMPANYCODE>207</ttow:cOMPANYCODE>
            <ttow:dEVICODE>16953126</ttow:dEVICODE>
            <ttow:fIRSTTOLASTAXLESLEN>1248</ttow:fIRSTTOLASTAXLESLEN>
            <ttow:gAP>0</ttow:gAP>
            <ttow:hEADGAP>0</ttow:hEADGAP>
            <ttow:hasImage>true</ttow:hasImage>
            <ttow:lENGTHAXLES12>368</ttow:lENGTHAXLES12>
            <ttow:lENGTHAXLES23>734</ttow:lENGTHAXLES23>
            <ttow:lENGTHAXLES34>146</ttow:lENGTHAXLES34>
            <ttow:lENGTHAXLES45>0</ttow:lENGTHAXLES45>
            <ttow:lENGTHAXLES56>0</ttow:lENGTHAXLES56>
            <ttow:lENGTHAXLES67>0</ttow:lENGTHAXLES67>
            <ttow:lENGTHAXLES78>0</ttow:lENGTHAXLES78>
            <ttow:lENGTHAXLESMORETHAN8>0</ttow:lENGTHAXLESMORETHAN8>
            <ttow:lINENUMBER>1</ttow:lINENUMBER>
            <ttow:pASSDATETIME>2026-08-08T05:00:22</ttow:pASSDATETIME>
            <ttow:pASSINFOID>14087</ttow:pASSINFOID>
            <ttow:pLATEIMAGE>BASE64_PLATE_IMAGE</ttow:pLATEIMAGE>
            <ttow:payment>0</ttow:payment>
            <ttow:rECEIVEDATETIME>2026-08-08T05:01:23</ttow:rECEIVEDATETIME>
            <ttow:rFIDNUMBER>FT538TF</ttow:rFIDNUMBER>
            <ttow:ReferenceNo>1748741421</ttow:ReferenceNo>
            <ttow:reserved7>607057</ttow:reserved7>
            <ttow:reserved8>0</ttow:reserved8>
            <ttow:sYSTEMCODE>202</ttow:sYSTEMCODE>
            <ttow:score>80</ttow:score>
            <ttow:speedType>1</ttow:speedType>
            <ttow:tOTALAXLES>4</ttow:tOTALAXLES>
            <ttow:tOTALOVERWEIGHT>0</ttow:tOTALOVERWEIGHT>
            <ttow:tOTALOVERWEIGHT_A>0</ttow:tOTALOVERWEIGHT_A>
            <ttow:tOTALOVERWEIGHT_AX1>0</ttow:tOTALOVERWEIGHT_AX1>
            <ttow:tOTALOVERWEIGHT_AX2>0</ttow:tOTALOVERWEIGHT_AX2>
            <ttow:tOTALOVERWEIGHT_AX3>0</ttow:tOTALOVERWEIGHT_AX3>
            <ttow:tOTALOVERWEIGHT_AX4>0</ttow:tOTALOVERWEIGHT_AX4>
            <ttow:tOTALOVERWEIGHT_AX5>0</ttow:tOTALOVERWEIGHT_AX5>
            <ttow:tOTALOVERWEIGHT_AX6>0</ttow:tOTALOVERWEIGHT_AX6>
            <ttow:tOTALOVERWEIGHT_AX7>0</ttow:tOTALOVERWEIGHT_AX7>
            <ttow:tOTALOVERWEIGHT_AX8>0</ttow:tOTALOVERWEIGHT_AX8>
            <ttow:tOTALOVERWEIGHT_AX_MORETHAN8>0</ttow:tOTALOVERWEIGHT_AX_MORETHAN8>
            <ttow:tOTALOVERWEIGHT_B>0</ttow:tOTALOVERWEIGHT_B>
            <ttow:tOTALOVERWEIGHT_C>0</ttow:tOTALOVERWEIGHT_C>
            <ttow:tOTALWEIGHT>15386</ttow:tOTALWEIGHT>
            <ttow:tOTALWEIGHT_A>4486</ttow:tOTALWEIGHT_A>
            <ttow:tOTALWEIGHT_B>4325</ttow:tOTALWEIGHT_B>
            <ttow:tOTALWEIGHT_C>6575</ttow:tOTALWEIGHT_C>
            <ttow:vEHICLECLASS>2</ttow:vEHICLECLASS>
            <ttow:vEHICLELEN>0</ttow:vEHICLELEN>
            <ttow:vEHICLEPLATE>0</ttow:vEHICLEPLATE>
            <ttow:vEHICLESPEED>99</ttow:vEHICLESPEED>
            <ttow:wRONGDIRECTION>0</ttow:wRONGDIRECTION>
            <ttow:PreviousDeviceCode>0</ttow:PreviousDeviceCode>
            <ttow:LPF>20</ttow:LPF>
            <ttow:SLPF>0</ttow:SLPF>
         </tem:TTOInfo>
      </tem:addTTOInfo>
   </soapenv:Body>
</soapenv:Envelope>'
```

---

## 6. addTTOInfoBatch2 — ارسال بستهای (پاسخ همان لحظه)

Max 100 records per batch. Array of `ttow:ttoInfo` elements (per WSDL schema), **no images** in batch mode.

SOAPAction: `addTTOInfoBatch2`

```bash
curl --location "http://10.30.197.140:8080/" \
--header "Content-Type: text/xml; charset=utf-8" \
--header "SOAPAction: addTTOInfoBatch2" \
--data-raw '<?xml version="1.0" encoding="utf-8"?>
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:tem="http://tempuri.org/" xmlns:ttow="http://schemas.datacontract.org/2004/07/TTOWS.Library.Models" xmlns:arr="http://schemas.microsoft.com/2003/10/Serialization/Arrays">
   <soapenv:Header/>
   <soapenv:Body>
      <tem:addTTOInfoBatch2>
         <tem:Token>TOKEN_FROM_LOGIN</tem:Token>
         <tem:TTOInfoItems>
            <ttow:ttoInfo>
               <ttow:Crimes></ttow:Crimes>
               <ttow:aLLOWED>1</ttow:aLLOWED>
               <ttow:cOMPANYCODE>207</ttow:cOMPANYCODE>
               <ttow:dEVICODE>16953126</ttow:dEVICODE>
               <ttow:lINENUMBER>1</ttow:lINENUMBER>
               <ttow:pASSDATETIME>2026-08-08T00:02:10</ttow:pASSDATETIME>
               <ttow:rECEIVEDATETIME>2026-08-08T00:04:13</ttow:rECEIVEDATETIME>
               <ttow:rFIDNUMBER>2953930گ</ttow:rFIDNUMBER>
               <ttow:ReferenceNo>32176190368</ttow:ReferenceNo>
               <ttow:reserved7>607057</ttow:reserved7>
               <ttow:sYSTEMCODE>202</ttow:sYSTEMCODE>
               <ttow:speedType>1</ttow:speedType>
               <ttow:vEHICLECLASS>1</ttow:vEHICLECLASS>
               <ttow:vEHICLELEN>0</ttow:vEHICLELEN>
               <ttow:vEHICLEPLATE>0</ttow:vEHICLEPLATE>
               <ttow:vEHICLESPEED>81</ttow:vEHICLESPEED>
               <ttow:wRONGDIRECTION>0</ttow:wRONGDIRECTION>
               <ttow:LPF>1</ttow:LPF>
               <ttow:SLPF>4</ttow:SLPF>
            </ttow:ttoInfo>
         </tem:TTOInfoItems>
      </tem:addTTOInfoBatch2>
   </soapenv:Body>
</soapenv:Envelope>'
```

Response `Result` is an array of `InquiryInfo` (one per record), each containing `REFERENCENO` and `VALIDATIONCODE`.

---

## 7. AddImage — ارسال عکس پلاک جدا از تردد

Send the cropped plate image separately (only allowed for normal/light traffic, up to 4 hours after the pass).

SOAPAction: `AddImage`

```bash
curl --location "http://10.30.197.140:8080/" \
--header "Content-Type: text/xml; charset=utf-8" \
--header "SOAPAction: AddImage" \
--data-raw '<?xml version="1.0" encoding="utf-8"?>
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:tem="http://tempuri.org/" xmlns:ttow="http://schemas.datacontract.org/2004/07/TTOWS.Library.Models">
   <soapenv:Header/>
   <soapenv:Body>
      <tem:AddImage>
         <tem:userName>tabatozin</tem:userName>
         <tem:password>tab@#Tozin#123</tem:password>
         <tem:images>
            <ttow:refNo>32176190368</ttow:refNo>
            <ttow:deviceID>16953126</ttow:deviceID>
            <ttow:passDateTime>2026-08-08T00:02:10</ttow:passDateTime>
            <ttow:allowed>1</ttow:allowed>
            <ttow:vehicleClass>1</ttow:vehicleClass>
            <ttow:colorImage>BASE64_COLOR_IMAGE</ttow:colorImage>
            <ttow:plateImage>BASE64_PLATE_IMAGE</ttow:plateImage>
         </tem:images>
      </tem:AddImage>
   </soapenv:Body>
</soapenv:Envelope>'
```

Note: `ImageInfo` element name in the WSDL is `image_info`; this body follows the document's class definition (chapter 5-3). Fields: RefNo, DeviceID, PassDateTime, Allowed, VehicleClass, COLORIMAGE, PLATEIMAGE, EXCOLORIMAGE1, EXCOLORIMAGE2.

---

## Notes & success codes (addTTOInfo)

| Code | Meaning |
|------|---------|
| 0    | ملی — plate accepted |
| 1    | پلاک مخفو |حdr acf accepted |
| 2    | پلاک ترانزیت |(|(|(|(|(|(| در |
| 99   | تخلف بدون عکس (warning, 4h to resend with image) |
| 101  | duplicate record |
| 113  | Pass/Receive DateTime expired — use current date |
| 1001 | token expired → re-Login |
| 1011 | method not allowed for this user |
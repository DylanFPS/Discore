import 'dart:html';
import 'dart:convert';
import 'dart:async';

import 'package:angular2/angular2.dart';
import 'package:angular2/security.dart';

import 'json_payload.dart';

@Component(
  selector: "discore-cta",
  templateUrl: 'cta.html',
  //styleUrls: const ['cta.css'],
  directives: const [COMMON_DIRECTIVES, SafeInnerHtmlDirective],
)
class CallToActionComponent implements OnInit {
  JsonPayload jsonPayload;
  DomSanitizationService _dss;

  CallToActionComponent(this._dss);

  ngOnInit() async {
    CustomEvent event = new CustomEvent('dartTrianglify', detail: JSON.encode({"elementID" : "ctaJumbo"}));
    document.dispatchEvent(event);

    jsonPayload = await getJson();
  }

  SafeUrl getZipball() {
    return _dss.bypassSecurityTrustUrl(jsonPayload.getZipURL());
  }

  String getVersion() {
    String v = jsonPayload.getTagName();
    return v.substring(1);
  }

  Future<dynamic> getJson() async {
    String response = await HttpRequest.getString('https://api.github.com/repos/BundledSticksInkorperated/Discore/releases/latest');
    return new JsonPayload(JSON.decode(response));
  }
}
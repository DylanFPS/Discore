import 'package:angular2/angular2.dart';
import 'package:angular2/security.dart';
import 'dart:html';
import 'dart:convert';
import 'dart:async';

import 'json_payload.dart';

@Component(
  selector: "cta",
  templateUrl: 'cta.html',
  //styleUrls: const ['cta.css'],
  directives: const [COMMON_DIRECTIVES, SafeInnerHtmlDirective],
)
class CallToActionComponent implements OnInit {
  JsonPayload jsonPayload;
  DomSanitizationService _dss;

  CallToActionComponent(this._dss);

  void ngOnInit() {
    CustomEvent event = new CustomEvent('dartTrianglify', detail: JSON.encode({"elementID" : "ctaJumbo"}));
    document.dispatchEvent(event);


    getJson()
      .then((data) {
        jsonPayload = data;
      });

  }

  SafeUrl getZipball() {
    return _dss.bypassSecurityTrustUrl(jsonPayload.getZipURL());
  }

  String getVersion() {
    String v = jsonPayload.getTagName();
    print('[CTA CONTROLLER] ${v.substring(1)}');
    return v.substring(1);
  }

  Future<dynamic> getJson() async {
    JsonPayload toReturn = null;
    await HttpRequest.getString('https://api.github.com/repos/BundledSticksInkorperated/Discore/releases/latest')
      .then((String res) {
        print(res);
        toReturn = new JsonPayload(JSON.decode(res));
      });
    return toReturn;
  }
}
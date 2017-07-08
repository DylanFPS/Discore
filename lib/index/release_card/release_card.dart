import 'dart:html';
import 'dart:async';
import 'dart:convert';

import 'package:angular2/angular2.dart';
import 'package:angular2/security.dart';

import 'json_payload.dart';

@Component(
  selector: 'discore-release-card',
  templateUrl: 'release_card.html',
  styleUrls: const ['release_card.css'],
  directives: const [COMMON_DIRECTIVES, SafeInnerHtmlDirective]
)
class ReleaseComponent implements OnInit {
  JsonPayload jsonPayload;
  /// Because by default angular uses `Element.appendHtml` on the databindings
  /// *and* removed `ng-bind-html-unsafe` in 1.2.0
  ///
  /// I now need to bypass that using `DomSanitizationService.bypassSecurityTrustHtml`
  /// which now turns the simple html string into a `SafeHtml` object,
  /// which pretty much is just a glorified string with a property on it that just says
  ///
  /// *"ya' don't worry about little old me. I'm a safe string, I swear"*
  ///
  /// I understand the want to protect people from injection but for fuck sakes google, document this somewhere other than change notes!
  ///
  /// or even better yet, add back `ng-bind-html-unsafe` as like `unsafeInnerHtml`
  ///
  /// atleast in pure dart it's easy to fix
  DomSanitizationService _dSS;

  ReleaseComponent(this._dSS);

  ngOnInit() async {
    jsonPayload = await getJson();
  }

  Future<JsonPayload> getJson() async {
    JsonPayload toReturn = new JsonPayload(_dSS);

    Stopwatch st = new Stopwatch();
    st.start();

    String response = await HttpRequest.getString('https://api.github.com/repos/BundledSticksInkorperated/Discore/releases/latest');
    st.stop();

    toReturn.data = JSON.decode(response);
    toReturn.stopWatch = st;

    return toReturn;
  }
}
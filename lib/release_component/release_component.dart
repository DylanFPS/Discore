import 'package:angular2/angular2.dart';
import 'package:angular2/security.dart';
import 'dart:html';
import 'dart:async';
import 'dart:convert';
import 'json_payload.dart';

@Component(
  selector: 'readme',
  templateUrl: 'release_component.html',
  styleUrls: const ['release_component.css'],
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

  void ngOnInit() {
    getJson()
      .then((JsonPayload data) {
        this.jsonPayload = data;
      });
  }

  Future<JsonPayload> getJson() async {
    JsonPayload toReturn = new JsonPayload(_dSS);
    Stopwatch st = new Stopwatch();
    st.start();
    await HttpRequest.getString('https://api.github.com/repos/BundledSticksInkorperated/Discore/releases/latest')
      .then((String res) {
        st.stop();
        toReturn.data = JSON.decode(res);
        toReturn.stopWatch = st;
      });
    return toReturn;
  }
}
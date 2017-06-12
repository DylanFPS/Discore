import 'package:angular2/angular2.dart';

import 'call_to_action_component/call_to_action_component.dart';
import 'release_component/release_component.dart';
import 'wiki_component/wiki_component.dart';
import 'footer_component/footer_component.dart';

@Component(
  selector: 'app',
  templateUrl: 'app_component.html',
  directives: const [COMMON_DIRECTIVES, CallToActionComponent, ReleaseComponent, WikiComponent, FooterComponent])
class AppComponent implements OnInit {
  void ngOnInit() {  }
}
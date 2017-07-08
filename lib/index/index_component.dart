import 'package:angular2/angular2.dart';

import 'release_card/release_card.dart';
import 'cta/cta.dart';

@Component(
  selector: 'discore-index',
  template: '''
    <discore-cta></discore-cta>
    <discore-release-card></discore-release-card>
  ''',
  directives: const [
    CallToActionComponent, 
    ReleaseComponent
  ]
)
class IndexComponent { }

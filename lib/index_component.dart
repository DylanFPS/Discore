import 'package:angular2/angular2.dart';

import 'release_component/release_component.dart';
import 'call_to_action_component/call_to_action_component.dart';

@Component(
    selector: 'dashboard',
    template: '''
    <cta></cta>
    <readme></readme>
    ''',
    directives: const [CallToActionComponent, ReleaseComponent]
)
class IndexComponent {}
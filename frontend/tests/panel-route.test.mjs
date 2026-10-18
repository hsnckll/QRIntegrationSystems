import test from 'node:test'
import assert from 'node:assert/strict'
import {readPanelRoute,panelHash} from '../src/shared/panel-route.js'
const pages=['home','businesses','subscriptions'],tabs=['info','menu','design']
test('panel deep route round trip',()=>{const route={page:'businesses',business:12,tab:'menu'};assert.deepEqual(readPanelRoute(panelHash(route),pages,'home',tabs),route)})
test('panel rejects malformed IDs',()=>{for(const id of ['-1','0','1.2','9007199254740993','abc'])assert.equal(readPanelRoute('#page=businesses&business='+id,pages,'home',tabs).business,null)})
test('panel rejects unknown page and tab',()=>{assert.equal(readPanelRoute('#page=evil',pages,'home',tabs).page,'home');assert.equal(readPanelRoute('#page=businesses&business=1&tab=evil',pages,'home',tabs).tab,'info')})
test('business detail does not leak across pages',()=>assert.equal(readPanelRoute('#page=subscriptions&business=12',pages,'home',tabs).business,null))

import {
  GoogleReCaptchaProvider,
} from "react-google-recaptcha-v3";
import { ContactForm } from "./ContactForm";
import { OrderForm } from "./OrderForm";

const CAPTCHA_SITE_KEY = import.meta.env.PUBLIC_RECAPTCHA_SITE_ID;

export default function ContactUsForms() {
  return (
    <GoogleReCaptchaProvider
      reCaptchaKey={CAPTCHA_SITE_KEY}
      scriptProps={{ async: true, defer: true }}
    >
      <h2 id="sign-up" data-phaeno-search="Stay Updated on Phaeno" className="mt-4 pb-2">Stay Updated on Phaeno</h2>
      <p>Product releases, validation updates, and technical insights. No spam.</p>
      <ContactForm />
      <h2 id="request-demo" data-phaeno-search="See PSeq in Action" className="mt-4 pb-2">See PSeq in Action</h2>
      <p>Walk through isoform-resolved RNA data with our team using your sample.</p>
      <OrderForm />
    </GoogleReCaptchaProvider>
  );
}
